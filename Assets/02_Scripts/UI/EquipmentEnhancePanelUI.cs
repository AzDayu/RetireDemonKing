using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentEnhancePanelUI : MonoBehaviour
{
    private const long BasePromotionCost = 50;

    [Header("장비 현재 등급 정보")]
    [SerializeField] private TMP_Text Text_EquipmentName;
    [SerializeField] private TMP_Text Text_Level;
    [SerializeField] private TMP_Text Text_Stat;
    [SerializeField] private TMP_Text Text_Currency;
    [SerializeField] private TMP_Text Text_EnhanceCost;
    [SerializeField] private UIButton Button_Enhance;

    [Header("장비 다음 등급 정보")]
    [SerializeField] private TMP_Text Text_NextLevel;
    [SerializeField] private TMP_Text Text_NextStat;

    [Header("장비 슬롯 버튼")]
    [SerializeField] private UIButton Button_Weapon;
    [SerializeField] private UIButton Button_Chest;
    [SerializeField] private UIButton Button_Pants;
    [SerializeField] private UIButton Button_Gloves;
    [SerializeField] private UIButton Button_Boots;
    [SerializeField] private UIButton Button_Belt;
    [SerializeField] private UIButton Button_Necklace;
    [SerializeField] private UIButton Button_Ring1;
    [SerializeField] private UIButton Button_Ring2;

    private readonly Dictionary<EquipmentSlotType, EquipmentItem>
        _equipmentDataMap =
            new Dictionary<EquipmentSlotType, EquipmentItem>();

    private readonly Dictionary<EquipmentSlotType, EquipmentModel>
        _equipmentModelMap =
            new Dictionary<EquipmentSlotType, EquipmentModel>();

    private EquipmentSlotType _selectedSlotType;
    private EquipmentItem _selectedEquipmentData;
    private EquipmentModel _selectedEquipmentModel;
    private PlayerModel _playerModel;

    private Coroutine _initializeCoroutine;

    private void OnEnable()
    {
        _initializeCoroutine =
            StartCoroutine(InitializeWhenDataReady());
    }

    private IEnumerator InitializeWhenDataReady()
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.Growth != null &&
            GameManager.Instance.Growth.PlayerModel != null &&
            GameManager.Instance.Data != null &&
            GameManager.Instance.Data.GetEquipmentData(
                "EQ_WEAPON_SWORD_Common"
            ) != null
        );

        InitializeEquipmentData();

        Button_Enhance?.UnBindAllOnClickButtonEvent();
        UnbindEquipmentSlotButtons();

        BindEquipmentSlotButtons();

        if (Button_Enhance != null)
        {
            Button_Enhance.BindOnClickButtonEvent(
                OnClick_Promote,
                true
            );
        }

        if (_selectedEquipmentData == null)
        {
            SelectEquipment(EquipmentSlotType.Chest);
        }
        else
        {
            RefreshUI();
        }

        _initializeCoroutine = null;
    }

    private void OnDisable()
    {
        if (_initializeCoroutine != null)
        {
            StopCoroutine(_initializeCoroutine);
            _initializeCoroutine = null;
        }

        Button_Enhance?.UnBindAllOnClickButtonEvent();
        UnbindEquipmentSlotButtons();
    }

    private void InitializeEquipmentData()
    {
        _playerModel = GameManager.Instance.Growth.PlayerModel;
        // 이미 9개 장비가 준비됐다면 강화 상태 유지
        if (_equipmentDataMap.Count == 9)
        {
            return;
        }

        _equipmentDataMap.Clear();
        _equipmentModelMap.Clear();

        RegisterEquipment(
            EquipmentSlotType.Weapon,
            "EQ_WEAPON_SWORD_Common",
            1
        );

        RegisterEquipment(
            EquipmentSlotType.Chest,
            "EQ_CHEST_ICE_Common",
            2
        );

        RegisterEquipment(
            EquipmentSlotType.Pants,
            "EQ_PANTS_GREEN_Common",
            3
        );

        RegisterEquipment(
            EquipmentSlotType.Gloves,
            "EQ_GLOVE_LEATHER_Common",
            4
        );

        RegisterEquipment(
            EquipmentSlotType.Boots,
            "EQ_BOOTS_BLACK_Common",
            5
        );

        RegisterEquipment(
            EquipmentSlotType.Belt,
            "EQ_BELT_TOOL_Common",
            6
        );

        RegisterEquipment(
            EquipmentSlotType.Necklace,
            "EQ_NECK_GREEN_Common",
            7
        );

        RegisterEquipment(
            EquipmentSlotType.Ring1,
            "EQ_RING_ICE_Common",
            8
        );

        RegisterEquipment(
            EquipmentSlotType.Ring2,
            "EQ_RING_FIRE_Common",
            9
        );
    }

    private void RegisterEquipment(
        EquipmentSlotType slotType,
        string equipmentDataId,
        long uniqueId)
    {
        EquipmentItem equipmentData =
            GameManager.Instance.Data.GetEquipmentData(
                equipmentDataId
            );

        if (equipmentData == null)
        {
            Debug.LogError(
                $"[장비 승급] 장비 데이터를 찾지 못했습니다: " +
                $"{equipmentDataId}"
            );

            return;
        }

        _equipmentDataMap[slotType] = equipmentData;

        _equipmentModelMap[slotType] = new EquipmentModel
        {
            ItemUniqueId = uniqueId,
            ItemDataId = equipmentDataId,
            Level = 1,
            IsEquipped = true
        };
    }

    private void BindEquipmentSlotButtons()
    {
        BindEquipmentSlotButton(
            Button_Weapon,
            EquipmentSlotType.Weapon
        );

        BindEquipmentSlotButton(
            Button_Chest,
            EquipmentSlotType.Chest
        );

        BindEquipmentSlotButton(
            Button_Pants,
            EquipmentSlotType.Pants
        );

        BindEquipmentSlotButton(
            Button_Gloves,
            EquipmentSlotType.Gloves
        );

        BindEquipmentSlotButton(
            Button_Boots,
            EquipmentSlotType.Boots
        );

        BindEquipmentSlotButton(
            Button_Belt,
            EquipmentSlotType.Belt
        );

        BindEquipmentSlotButton(
            Button_Necklace,
            EquipmentSlotType.Necklace
        );

        BindEquipmentSlotButton(
            Button_Ring1,
            EquipmentSlotType.Ring1
        );

        BindEquipmentSlotButton(
            Button_Ring2,
            EquipmentSlotType.Ring2
        );
    }

    private void BindEquipmentSlotButton(
        UIButton button,
        EquipmentSlotType slotType)
    {
        if (button == null)
        {
            Debug.LogWarning(
                $"[장비 승급] 슬롯 버튼이 연결되지 않았습니다: " +
                $"{slotType}"
            );

            return;
        }

        button.BindOnClickButtonEvent(
            () => SelectEquipment(slotType),
            true
        );
    }

    private void UnbindEquipmentSlotButtons()
    {
        Button_Weapon?.UnBindAllOnClickButtonEvent();
        Button_Chest?.UnBindAllOnClickButtonEvent();
        Button_Pants?.UnBindAllOnClickButtonEvent();
        Button_Gloves?.UnBindAllOnClickButtonEvent();
        Button_Boots?.UnBindAllOnClickButtonEvent();
        Button_Belt?.UnBindAllOnClickButtonEvent();
        Button_Necklace?.UnBindAllOnClickButtonEvent();
        Button_Ring1?.UnBindAllOnClickButtonEvent();
        Button_Ring2?.UnBindAllOnClickButtonEvent();
    }

    private void SelectEquipment(
        EquipmentSlotType slotType)
    {
        if (!_equipmentDataMap.TryGetValue(
                slotType,
                out EquipmentItem equipmentData) ||
            !_equipmentModelMap.TryGetValue(
                slotType,
                out EquipmentModel equipmentModel))
        {
            Debug.LogWarning(
                $"[장비 승급] 등록되지 않은 장비 슬롯입니다: " +
                $"{slotType}"
            );

            return;
        }

        _selectedSlotType = slotType;
        _selectedEquipmentData = equipmentData;
        _selectedEquipmentModel = equipmentModel;

        RefreshUI();
    }

    private void OnClick_Promote()
    {
        if (_selectedEquipmentData == null ||
            _selectedEquipmentModel == null ||
            _playerModel == null)
        {
            return;
        }

        EquipmentItem nextGradeData =
            FindNextGradeEquipment(
                _selectedEquipmentData
            );

        if (nextGradeData == null)
        {
            Debug.Log("[장비 승급] 이미 최고 등급입니다.");
            return;
        }

        long promotionCost = CalculatePromotionCost();

        if (_playerModel.EnhanceCurrency < promotionCost)
        {
            Debug.Log("[장비 승급] 승급 재화가 부족합니다.");
            return;
        }

        _playerModel.EnhanceCurrency -= promotionCost;

        // 같은 장비 모델의 데이터 ID만 다음 등급으로 교체
        _selectedEquipmentModel.ItemDataId =
            nextGradeData.Id;

        _selectedEquipmentData = nextGradeData;

        _equipmentDataMap[_selectedSlotType] =
            nextGradeData;

        RefreshUI();
    }

    private EquipmentItem FindNextGradeEquipment(
     EquipmentItem currentData)
    {
        if (currentData == null ||
            string.IsNullOrEmpty(currentData.Id))
        {
            return null;
        }

        string currentId = currentData.Id;
        string nextGradeSuffix;

        if (currentId.EndsWith(
                "_Common",
                StringComparison.OrdinalIgnoreCase))
        {
            nextGradeSuffix = "RARE";
        }
        else if (currentId.EndsWith(
                     "_RARE",
                     StringComparison.OrdinalIgnoreCase))
        {
            nextGradeSuffix = "EPIC";
        }
        else if (currentId.EndsWith(
                     "_EPIC",
                     StringComparison.OrdinalIgnoreCase))
        {
            nextGradeSuffix = "LEGENDARY";
        }
        else if (currentId.EndsWith(
                     "_LEGENDARY",
                     StringComparison.OrdinalIgnoreCase))
        {
            nextGradeSuffix = "MYTHIC";
        }
        else if (currentId.EndsWith(
                     "_MYTHIC",
                     StringComparison.OrdinalIgnoreCase))
        {
            // 신화 등급은 다음 등급이 없음
            return null;
        }
        else
        {
            Debug.LogError(
                $"[장비 승급] 장비 ID에서 등급을 확인할 수 없습니다: " +
                $"{currentId}"
            );

            return null;
        }

        string equipmentFamilyId =
            GetEquipmentFamilyId(currentId);

        string nextEquipmentId =
            $"{equipmentFamilyId}_{nextGradeSuffix}";

        EquipmentItem nextGradeData =
            GameManager.Instance.Data.GetEquipmentData(
                nextEquipmentId
            );

        if (nextGradeData == null)
        {
            Debug.LogError(
                $"[장비 승급] 다음 등급 데이터를 찾지 못했습니다: " +
                $"{nextEquipmentId}"
            );

            return null;
        }

        return nextGradeData;
    }

    private string GetEquipmentFamilyId(
        string equipmentId)
    {
        if (string.IsNullOrEmpty(equipmentId))
        {
            return string.Empty;
        }

        int lastSeparatorIndex =
            equipmentId.LastIndexOf('_');

        if (lastSeparatorIndex <= 0)
        {
            return equipmentId;
        }

        return equipmentId.Substring(
            0,
            lastSeparatorIndex
        );
    }

    private EquipmentGrade GetGradeFromId(
        string equipmentId)
    {
        if (string.IsNullOrEmpty(equipmentId))
        {
            Debug.LogError("[장비 승급] 장비 ID가 비어 있습니다.");
            return EquipmentGrade.Common;
        }

        if (equipmentId.EndsWith(
                "_Common",
                StringComparison.OrdinalIgnoreCase))
        {
            return EquipmentGrade.Common;
        }

        if (equipmentId.EndsWith(
                "_RARE",
                StringComparison.OrdinalIgnoreCase))
        {
            return EquipmentGrade.Rare;
        }

        if (equipmentId.EndsWith(
                "_EPIC",
                StringComparison.OrdinalIgnoreCase))
        {
            return EquipmentGrade.Epic;
        }

        if (equipmentId.EndsWith(
                "_LEGENDARY",
                StringComparison.OrdinalIgnoreCase))
        {
            return EquipmentGrade.Legendary;
        }

        if (equipmentId.EndsWith(
                "_MYTHIC",
                StringComparison.OrdinalIgnoreCase))
        {
            return EquipmentGrade.Mythic;
        }

        Debug.LogError(
            $"[장비 승급] 장비 ID에서 등급을 확인할 수 없습니다: " +
            $"{equipmentId}"
        );

        return EquipmentGrade.Common;
    }

    private long CalculatePromotionCost()
    {
        if (_selectedEquipmentData == null)
        {
            return 0;
        }

        EquipmentGrade currentGrade =
            GetGradeFromId(_selectedEquipmentData.Id);

        int gradeWeight =
            (int)currentGrade + 1;

        return BasePromotionCost * gradeWeight;
    }

    private void RefreshUI()
    {
        if (_selectedEquipmentData == null ||
            _selectedEquipmentModel == null ||
            _playerModel == null)
        {
            return;
        }

        EquipmentItem nextGradeData =
            FindNextGradeEquipment(
                _selectedEquipmentData
            );

        float currentStat =
            CalculateStat(_selectedEquipmentData);

        string statName =
            GetStatDisplayName(_selectedSlotType);

        if (Text_EquipmentName != null)
        {
            Text_EquipmentName.text =
                _selectedEquipmentData.Name;
        }

        if (Text_Level != null)
        {
            Text_Level.text =
                GetGradeDisplayName(
                    GetGradeFromId(
                        _selectedEquipmentData.Id
                    )
                );
        }

        if (Text_Stat != null)
        {
            Text_Stat.text =
                $"{statName}: {currentStat:0.##}";
        }

        if (Text_Currency != null)
        {
            Text_Currency.text =
                $"승급 재화: " +
                $"{_playerModel.EnhanceCurrency:N0}";
        }

        if (nextGradeData == null)
        {
            if (Text_NextLevel != null)
            {
                Text_NextLevel.text = "MAX";
            }

            if (Text_NextStat != null)
            {
                Text_NextStat.text = "최고 등급";
            }

            if (Text_EnhanceCost != null)
            {
                Text_EnhanceCost.text = "승급 비용: -";
            }

            if (Button_Enhance != null)
            {
                Button_Enhance.gameObject.SetActive(false);
            }

            return;
        }

        float nextStat =
            CalculateStat(nextGradeData);

        float increaseStat =
            nextStat - currentStat;

        if (Text_NextLevel != null)
        {
            Text_NextLevel.text =
                GetGradeDisplayName(
                    GetGradeFromId(nextGradeData.Id)
                );
        }

        if (Text_NextStat != null)
        {
            Text_NextStat.text =
                $"{statName}: {nextStat:0.##} " +
                $"<color=#67E480>" +
                $"(+{increaseStat:0.##})" +
                $"</color>";
        }

        if (Text_EnhanceCost != null)
        {
            Text_EnhanceCost.text =
                $"승급 비용: " +
                $"{CalculatePromotionCost():N0}";
        }

        if (Button_Enhance != null)
        {
            Button_Enhance.gameObject.SetActive(true);
        }
    }

    private float CalculateStat(
        EquipmentItem equipmentData)
    {
        if (equipmentData == null)
        {
            return 0f;
        }

        return equipmentData.BaseStatValue *
               equipmentData.GradeMultiplier;
    }

    private string GetGradeDisplayName(
        EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.Common:
                return "일반";

            case EquipmentGrade.Rare:
                return "고급";

            case EquipmentGrade.Epic:
                return "희귀";

            case EquipmentGrade.Legendary:
                return "전설";

            case EquipmentGrade.Mythic:
                return "신화";

            default:
                return grade.ToString();
        }
    }

    private string GetStatDisplayName(
        EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                return "공격력";

            case EquipmentSlotType.Chest:
                return "방어력";

            case EquipmentSlotType.Pants:
                return "체력";

            case EquipmentSlotType.Gloves:
                return "공격력";

            case EquipmentSlotType.Boots:
                return "이동 속도";

            case EquipmentSlotType.Belt:
                return "명중률";

            case EquipmentSlotType.Necklace:
                return "체력";

            case EquipmentSlotType.Ring1:
                return "재사용 대기시간 감소";

            case EquipmentSlotType.Ring2:
                return "치명타 피해";

            default:
                return "능력치";
        }
    }
}
