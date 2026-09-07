using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;


public class MainHUDUI : UIBase
{
    [SerializeField] private Button _buttonMenu;
    [SerializeField] private Button _buttonRebirth;
    [SerializeField] private Slider _playerHpSlider;
    [SerializeField] private TMPro.TMP_Text _playerHpText;

    private PlayerController _playerController;
    private GrowthManager _subscribedGrowthManager;
    private Coroutine _equipmentInitializeCoroutine;
    private int _equipmentIconRefreshVersion;
    private readonly Dictionary<EquipmentSlotType, Transform> _equipmentSlotMap = new();

    private void Awake()
    {
        InitUIButton();
    }

    private void OnEnable()
    {
        BindPlayerHealth();
        _equipmentInitializeCoroutine = StartCoroutine(InitializeEquipmentUIWhenReady());
    }

    private void OnDisable()
    {
        UnbindPlayerHealth();
        UnbindEquipmentUpdates();

        if (_equipmentInitializeCoroutine != null)
        {
            StopCoroutine(_equipmentInitializeCoroutine);
            _equipmentInitializeCoroutine = null;
        }

        _equipmentIconRefreshVersion++;
    }

    private IEnumerator InitializeEquipmentUIWhenReady()
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.Growth != null &&
            GameManager.Instance.Growth.IsInitialized &&
            GameManager.Instance.Growth.Equipment != null &&
            GameManager.Instance.Data != null &&
            GameManager.Instance.Resource != null);

        CacheEquipmentSlots();
        BindEquipmentUpdates();
        RefreshEquipmentSlots();
        _equipmentInitializeCoroutine = null;
    }

    private void CacheEquipmentSlots()
    {
        _equipmentSlotMap.Clear();
        RegisterEquipmentSlot(EquipmentSlotType.Weapon, "Slot_Weapon");
        RegisterEquipmentSlot(EquipmentSlotType.Chest, "Slot_Chest");
        RegisterEquipmentSlot(EquipmentSlotType.Pants, "Slot_Pants");
        RegisterEquipmentSlot(EquipmentSlotType.Gloves, "Slot_Gloves");
        RegisterEquipmentSlot(EquipmentSlotType.Boots, "Slot_Boots");
        RegisterEquipmentSlot(EquipmentSlotType.Belt, "Slot_Belt");
        RegisterEquipmentSlot(EquipmentSlotType.Necklace, "Slot_Necklace");
        RegisterEquipmentSlot(EquipmentSlotType.Ring1, "Slot_Ring1");
        RegisterEquipmentSlot(EquipmentSlotType.Ring2, "Slot_Ring2");
    }

    private void RegisterEquipmentSlot(EquipmentSlotType slotType, string objectName)
    {
        Transform slot = EquipmentSlotIconUI.FindSlot(transform, objectName);
        if (slot == null)
        {
            Debug.LogWarning($"[장비 UI] 메인 HUD 슬롯을 찾지 못했습니다: {objectName}");
            return;
        }

        _equipmentSlotMap[slotType] = slot;
    }

    private void BindEquipmentUpdates()
    {
        UnbindEquipmentUpdates();
        _subscribedGrowthManager = GameManager.Instance?.Growth;
        if (_subscribedGrowthManager != null)
            _subscribedGrowthManager.OnStatsUpdated += RefreshEquipmentSlots;
    }

    private void UnbindEquipmentUpdates()
    {
        if (_subscribedGrowthManager == null) return;
        _subscribedGrowthManager.OnStatsUpdated -= RefreshEquipmentSlots;
        _subscribedGrowthManager = null;
    }

    private void RefreshEquipmentSlots()
    {
        EquipmentManager equipmentManager = GameManager.Instance?.Growth?.Equipment;
        GameDataManager dataManager = GameManager.Instance?.Data;
        if (equipmentManager == null || dataManager == null) return;

        int refreshVersion = ++_equipmentIconRefreshVersion;

        foreach (var entry in _equipmentSlotMap)
        {
            GetEquipmentLookup(entry.Key, out EquipmentType equipmentType, out int typeIndex);
            EquipmentModel model = equipmentManager.GetEquippedEquipment(equipmentType, typeIndex);
            EquipmentItem data = model != null
                ? dataManager.GetEquipmentData(model.ItemDataId)
                : null;

            EquipmentSlotIconUI.ShowEmpty(entry.Value);

            if (model == null || data == null || string.IsNullOrEmpty(data.IconId)) continue;
            LoadEquipmentIconAsync(
                entry.Key,
                entry.Value,
                model,
                data.IconId,
                data.Grade,
                refreshVersion
            ).Forget();
        }
    }

    private async UniTaskVoid LoadEquipmentIconAsync(
        EquipmentSlotType slotType,
        Transform slotRoot,
        EquipmentModel model,
        string iconId,
        EquipmentGrade grade,
        int refreshVersion)
    {
        Sprite sprite = await GameManager.Instance.Resource.LoadSprite(iconId);

        if (this == null || !isActiveAndEnabled || refreshVersion != _equipmentIconRefreshVersion)
            return;

        GetEquipmentLookup(slotType, out EquipmentType equipmentType, out int typeIndex);
        EquipmentModel currentModel = GameManager.Instance?.Growth?.Equipment
            ?.GetEquippedEquipment(equipmentType, typeIndex);
        if (!ReferenceEquals(currentModel, model)) return;

        if (!EquipmentSlotIconUI.ShowEquipment(slotRoot, sprite, model.Level, grade))
            Debug.LogWarning($"[장비 UI] 메인 HUD 아이콘 표시 실패: {model.ItemDataId} / {iconId}");
    }

    private static void GetEquipmentLookup(
        EquipmentSlotType slotType,
        out EquipmentType equipmentType,
        out int typeIndex)
    {
        typeIndex = 0;
        equipmentType = slotType switch
        {
            EquipmentSlotType.Weapon => EquipmentType.Weapon,
            EquipmentSlotType.Chest => EquipmentType.Chest,
            EquipmentSlotType.Pants => EquipmentType.Pants,
            EquipmentSlotType.Gloves => EquipmentType.Gloves,
            EquipmentSlotType.Boots => EquipmentType.Boots,
            EquipmentSlotType.Belt => EquipmentType.Belt,
            EquipmentSlotType.Necklace => EquipmentType.Necklace,
            EquipmentSlotType.Ring2 => EquipmentType.Ring,
            _ => EquipmentType.Ring
        };

        if (slotType == EquipmentSlotType.Ring2) typeIndex = 1;
    }

    private void InitUIButton()
    {
        if (_buttonMenu != null)
        {
            _buttonMenu.onClick.RemoveAllListeners();
            _buttonMenu.onClick.AddListener(OnClickMenu);
        }
        if (_buttonRebirth != null)
        {
            _buttonRebirth.onClick.RemoveAllListeners();
            _buttonRebirth.onClick.AddListener(OnClickRebirth);
        }
    }

    private void BindPlayerHealth()
    {
        UnbindPlayerHealth();
        _playerController = PlayerController.Instance;

        if (_playerController == null)
        {
            RefreshPlayerHealth(0f, 0f);
            return;
        }

        _playerController.OnHpChanged += RefreshPlayerHealth;
        RefreshPlayerHealth(
            _playerController.CurHp,
            _playerController.MaxHp
        );
    }

    private void UnbindPlayerHealth()
    {
        if (_playerController != null)
        {
            _playerController.OnHpChanged -= RefreshPlayerHealth;
            _playerController = null;
        }
    }

    private void RefreshPlayerHealth(float currentHp, float maxHp)
    {
        float clampedMaxHp = Mathf.Max(0f, maxHp);
        float clampedCurrentHp = Mathf.Clamp(
            currentHp,
            0f,
            clampedMaxHp
        );

        if (_playerHpSlider != null)
        {
            _playerHpSlider.value = clampedMaxHp > 0f
                ? clampedCurrentHp / clampedMaxHp
                : 0f;
        }

        if (_playerHpText != null)
        {
            _playerHpText.text =
                $"{Mathf.CeilToInt(clampedCurrentHp):N0}/" +
                $"{Mathf.CeilToInt(clampedMaxHp):N0}";
        }
    }

    private void OnClickMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.OpenMenuPopupUI();
        }
    }
    private void OnClickRebirth()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.OpenRebirthPopupUI();
        }
    }
}
