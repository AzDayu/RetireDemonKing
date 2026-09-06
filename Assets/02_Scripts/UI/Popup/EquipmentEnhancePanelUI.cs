using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 현재 장착 중인 장비를 슬롯별로 보여주고, 선택한 장비의 등급 승급을 처리한다.
/// EquipmentItem은 정적 데이터, EquipmentModel은 플레이어가 보유한 장비 상태를 의미한다.
/// </summary>
public class EquipmentEnhancePanelUI : UIBase
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

    // 동일한 슬롯을 기준으로 표시용 정적 데이터와 실제 보유 장비 상태를 각각 보관한다.
    private readonly Dictionary<EquipmentSlotType, EquipmentItem> _equipmentDataMap = new();
    private readonly Dictionary<EquipmentSlotType, EquipmentModel> _equipmentModelMap = new();

    private EquipmentSlotType _selectedSlotType;
    private EquipmentItem _selectedEquipmentData;
    private EquipmentModel _selectedEquipmentModel;
    private PlayerModel _playerModel;
    private Coroutine _initializeCoroutine;

    private void OnEnable()
    {
        _initializeCoroutine = StartCoroutine(InitializeWhenDataReady());
    }

    private IEnumerator InitializeWhenDataReady()
    {
        // 세이브 데이터와 장비 원본 데이터가 모두 준비된 뒤 UI를 구성한다.
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.Growth != null &&
            GameManager.Instance.Growth.IsInitialized &&
            GameManager.Instance.Growth.PlayerModel != null &&
            GameManager.Instance.Growth.Equipment != null &&
            GameManager.Instance.Data != null &&
            GameManager.Instance.Data.GetAllEquipmentDataList().Count > 0);

        InitializeEquipmentData();
        UnbindButtons();
        BindButtons();

        if (_selectedEquipmentData == null || !_equipmentDataMap.ContainsKey(_selectedSlotType))
            SelectDefaultEquipment();
        else
            SelectEquipment(_selectedSlotType);

        _initializeCoroutine = null;
    }

    private void OnDisable()
    {
        if (_initializeCoroutine != null)
        {
            StopCoroutine(_initializeCoroutine);
            _initializeCoroutine = null;
        }

        UnbindButtons();
    }

    private void InitializeEquipmentData()
    {
        _playerModel = GameManager.Instance.Growth.PlayerModel;
        _equipmentDataMap.Clear();
        _equipmentModelMap.Clear();

        // 반지는 같은 EquipmentType을 사용하므로 조회 순서(0, 1)로 두 슬롯을 구분한다.
        RegisterEquipment(EquipmentSlotType.Weapon, EquipmentType.Weapon);
        RegisterEquipment(EquipmentSlotType.Chest, EquipmentType.Chest);
        RegisterEquipment(EquipmentSlotType.Pants, EquipmentType.Pants);
        RegisterEquipment(EquipmentSlotType.Gloves, EquipmentType.Gloves);
        RegisterEquipment(EquipmentSlotType.Boots, EquipmentType.Boots);
        RegisterEquipment(EquipmentSlotType.Belt, EquipmentType.Belt);
        RegisterEquipment(EquipmentSlotType.Necklace, EquipmentType.Necklace);
        RegisterEquipment(EquipmentSlotType.Ring1, EquipmentType.Ring, 0);
        RegisterEquipment(EquipmentSlotType.Ring2, EquipmentType.Ring, 1);
    }

    private void RegisterEquipment(EquipmentSlotType slotType, EquipmentType equipmentType, int typeIndex = 0)
    {
        EquipmentModel model = GameManager.Instance.Growth.Equipment
            .GetEquippedEquipment(equipmentType, typeIndex);
        if (model == null) return;

        EquipmentItem data = GameManager.Instance.Data.GetEquipmentData(model.ItemDataId);
        if (data == null)
        {
            Debug.LogWarning($"[장비 승급] 장비 데이터를 찾지 못했습니다: {model.ItemDataId}");
            return;
        }

        _equipmentDataMap[slotType] = data;
        _equipmentModelMap[slotType] = model;
    }

    private void SelectDefaultEquipment()
    {
        // 상의를 우선 선택하고, 없다면 enum 순서상 첫 번째 장착 장비를 선택한다.
        if (_equipmentDataMap.ContainsKey(EquipmentSlotType.Chest))
        {
            SelectEquipment(EquipmentSlotType.Chest);
            return;
        }

        foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
        {
            if (!_equipmentDataMap.ContainsKey(slotType)) continue;
            SelectEquipment(slotType);
            return;
        }
    }

    private IEnumerable<(UIButton Button, EquipmentSlotType Slot)> GetSlotButtons()
    {
        // 버튼 바인딩과 해제에서 동일한 슬롯 목록을 사용하기 위한 단일 매핑이다.
        yield return (Button_Weapon, EquipmentSlotType.Weapon);
        yield return (Button_Chest, EquipmentSlotType.Chest);
        yield return (Button_Pants, EquipmentSlotType.Pants);
        yield return (Button_Gloves, EquipmentSlotType.Gloves);
        yield return (Button_Boots, EquipmentSlotType.Boots);
        yield return (Button_Belt, EquipmentSlotType.Belt);
        yield return (Button_Necklace, EquipmentSlotType.Necklace);
        yield return (Button_Ring1, EquipmentSlotType.Ring1);
        yield return (Button_Ring2, EquipmentSlotType.Ring2);
    }

    private void BindButtons()
    {
        foreach (var entry in GetSlotButtons())
        {
            if (entry.Button == null)
            {
                Debug.LogWarning($"[장비 승급] 슬롯 버튼이 연결되지 않았습니다: {entry.Slot}");
                continue;
            }

            // 각 콜백이 반복문의 현재 슬롯 값을 확실히 기억하도록 복사한다.
            EquipmentSlotType capturedSlot = entry.Slot;
            entry.Button.BindOnClickButtonEvent(() => SelectEquipment(capturedSlot), true);
        }

        Button_Enhance?.BindOnClickButtonEvent(OnClick_Promote, true);
    }

    private void UnbindButtons()
    {
        Button_Enhance?.UnBindAllOnClickButtonEvent();
        foreach (var entry in GetSlotButtons())
            entry.Button?.UnBindAllOnClickButtonEvent();
    }

    private void SelectEquipment(EquipmentSlotType slotType)
    {
        if (!_equipmentDataMap.TryGetValue(slotType, out EquipmentItem data) ||
            !_equipmentModelMap.TryGetValue(slotType, out EquipmentModel model))
        {
            Debug.LogWarning($"[장비 승급] 등록되지 않은 장비 슬롯입니다: {slotType}");
            return;
        }

        _selectedSlotType = slotType;
        _selectedEquipmentData = data;
        _selectedEquipmentModel = model;
        RefreshUI();
    }

    private void OnClick_Promote()
    {
        if (_selectedEquipmentData == null || _selectedEquipmentModel == null || _playerModel == null)
            return;

        EquipmentItem nextGradeData = FindNextGradeEquipment(_selectedEquipmentData);
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

        // 승급 성공 여부와 관계없이 도전 비용은 먼저 차감한다.
        _playerModel.EnhanceCurrency -= promotionCost;

        int successRatePercent = GetPromotionSuccessRatePercent();
        int randomValue = UnityEngine.Random.Range(0, 100);
        if (randomValue >= successRatePercent)
        {
            Debug.Log($"[장비 승급] 실패! 성공 확률: {successRatePercent}%, 판정값: {randomValue}");
            RefreshUI();
            return;
        }

        Debug.Log($"[장비 승급] 성공! 성공 확률: {successRatePercent}%, 판정값: {randomValue}");

        // 보유 장비 객체는 유지하고 참조하는 정적 데이터 ID만 다음 등급으로 교체한다.
        _selectedEquipmentModel.ItemDataId = nextGradeData.Id;
        _selectedEquipmentData = nextGradeData;
        _equipmentDataMap[_selectedSlotType] = nextGradeData;

        GameManager.Instance.Growth.RecalculateTotalStats();
        RefreshUI();
    }

    private EquipmentItem FindNextGradeEquipment(EquipmentItem currentData)
    {
        if (currentData == null || string.IsNullOrEmpty(currentData.Id)) return null;

        string nextGradeSuffix = GetNextGradeSuffix(currentData.Id);
        if (nextGradeSuffix == null) return null;

        // 예: EQ_WEAPON_SWORD_Common -> EQ_WEAPON_SWORD_RARE
        string nextEquipmentId = $"{GetEquipmentFamilyId(currentData.Id)}_{nextGradeSuffix}";
        EquipmentItem nextGradeData = GameManager.Instance.Data.GetEquipmentData(nextEquipmentId);

        if (nextGradeData == null)
            Debug.LogError($"[장비 승급] 다음 등급 데이터를 찾지 못했습니다: {nextEquipmentId}");

        return nextGradeData;
    }

    private string GetNextGradeSuffix(string equipmentId)
    {
        // 현재 장비 데이터는 ID 마지막 접미사로 등급 계열을 구분한다.
        if (equipmentId.EndsWith("_Common", StringComparison.OrdinalIgnoreCase)) return "RARE";
        if (equipmentId.EndsWith("_RARE", StringComparison.OrdinalIgnoreCase)) return "EPIC";
        if (equipmentId.EndsWith("_EPIC", StringComparison.OrdinalIgnoreCase)) return "LEGENDARY";
        if (equipmentId.EndsWith("_LEGENDARY", StringComparison.OrdinalIgnoreCase)) return "MYTHIC";
        if (equipmentId.EndsWith("_MYTHIC", StringComparison.OrdinalIgnoreCase)) return null;

        Debug.LogError($"[장비 승급] 장비 ID에서 등급을 확인할 수 없습니다: {equipmentId}");
        return null;
    }

    private string GetEquipmentFamilyId(string equipmentId)
    {
        if (string.IsNullOrEmpty(equipmentId)) return string.Empty;
        int separatorIndex = equipmentId.LastIndexOf('_');
        return separatorIndex > 0 ? equipmentId.Substring(0, separatorIndex) : equipmentId;
    }

    private EquipmentGrade GetGradeFromId(string equipmentId)
    {
        if (string.IsNullOrEmpty(equipmentId))
        {
            Debug.LogError("[장비 승급] 장비 ID가 비어 있습니다.");
            return EquipmentGrade.Common;
        }

        if (equipmentId.EndsWith("_Common", StringComparison.OrdinalIgnoreCase)) return EquipmentGrade.Common;
        if (equipmentId.EndsWith("_RARE", StringComparison.OrdinalIgnoreCase)) return EquipmentGrade.Rare;
        if (equipmentId.EndsWith("_EPIC", StringComparison.OrdinalIgnoreCase)) return EquipmentGrade.Epic;
        if (equipmentId.EndsWith("_LEGENDARY", StringComparison.OrdinalIgnoreCase)) return EquipmentGrade.Legendary;
        if (equipmentId.EndsWith("_MYTHIC", StringComparison.OrdinalIgnoreCase)) return EquipmentGrade.Mythic;

        Debug.LogError($"[장비 승급] 장비 ID에서 등급을 확인할 수 없습니다: {equipmentId}");
        return EquipmentGrade.Common;
    }

    private long CalculatePromotionCost()
    {
        if (_selectedEquipmentData == null) return 0;

        // 일반부터 등급 순서대로 기본 비용의 1~5배를 적용한다.
        return BasePromotionCost * ((int)GetGradeFromId(_selectedEquipmentData.Id) + 1);
    }

    private int GetPromotionSuccessRatePercent()
    {
        if (_selectedEquipmentData == null) return 0;

        // 다음 등급으로 갈수록 성공 확률이 낮아지며, 최고 등급은 승급할 수 없다.
        return GetGradeFromId(_selectedEquipmentData.Id) switch
        {
            EquipmentGrade.Common => 20,
            EquipmentGrade.Rare => 10,
            EquipmentGrade.Epic => 5,
            EquipmentGrade.Legendary => 3,
            _ => 0
        };
    }

    private void RefreshUI()
    {
        if (_selectedEquipmentData == null || _selectedEquipmentModel == null || _playerModel == null)
            return;

        // 현재 등급과 다음 등급을 함께 계산해 비교 영역을 한 번에 갱신한다.
        EquipmentItem nextGradeData = FindNextGradeEquipment(_selectedEquipmentData);
        float currentStat = CalculateStat(_selectedEquipmentData);
        string statName = GetStatDisplayName(_selectedEquipmentData.MainStatType);

        SetText(Text_EquipmentName, _selectedEquipmentData.Name);
        SetText(Text_Level, GetGradeDisplayName(GetGradeFromId(_selectedEquipmentData.Id)));
        SetText(Text_Stat, $"{statName}: {currentStat:0.##}");
        SetText(Text_Currency, $"승급 재화: {_playerModel.EnhanceCurrency:N0}");

        if (nextGradeData == null)
        {
            // 다음 데이터가 없는 신화 등급에서는 승급 버튼을 숨긴다.
            SetText(Text_NextLevel, "MAX");
            SetText(Text_NextStat, "최고 등급");
            SetText(Text_EnhanceCost, "승급 비용: -");
            if (Button_Enhance != null) Button_Enhance.gameObject.SetActive(false);
            return;
        }

        float nextStat = CalculateStat(nextGradeData);
        float increaseStat = nextStat - currentStat;

        SetText(Text_NextLevel, GetGradeDisplayName(GetGradeFromId(nextGradeData.Id)));
        SetText(Text_NextStat,
            $"{statName}: {nextStat:0.##} <color=#67E480>(+{increaseStat:0.##})</color>");
        SetText(Text_EnhanceCost, $"승급 비용: {CalculatePromotionCost():N0}");
        if (Button_Enhance != null) Button_Enhance.gameObject.SetActive(true);
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null) text.text = value;
    }

    private static float CalculateStat(EquipmentItem data)
    {
        // 이 화면의 승급 비교값은 현재 기획대로 기본 능력치와 등급 배율만 반영한다.
        return data == null ? 0f : data.BaseStatValue * data.GradeMultiplier;
    }

    private static string GetGradeDisplayName(EquipmentGrade grade)
    {
        return grade switch
        {
            EquipmentGrade.Common => "일반",
            EquipmentGrade.Rare => "고급",
            EquipmentGrade.Epic => "희귀",
            EquipmentGrade.Legendary => "전설",
            EquipmentGrade.Mythic => "신화",
            _ => grade.ToString()
        };
    }

    private static string GetStatDisplayName(StatType statType)
    {
        return statType switch
        {
            StatType.Attack => "공격력",
            StatType.MaxHp => "체력",
            StatType.Defense => "방어력",
            StatType.CriticalChance => "치명타 확률",
            StatType.CriticalDamage => "치명타 피해",
            StatType.AttackSpeed => "공격 속도",
            StatType.CooldownReduction => "재사용 대기시간 감소",
            StatType.Accuracy => "명중률",
            StatType.Evasion => "회피율",
            StatType.LifeSteal => "생명력 흡수",
            StatType.MoveSpeed => "이동 속도",
            _ => statType.ToString()
        };
    }
}
