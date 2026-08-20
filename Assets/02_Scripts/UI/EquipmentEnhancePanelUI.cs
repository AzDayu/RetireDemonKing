using TMPro;
using UnityEngine;

public class EquipmentEnhancePanelUI : MonoBehaviour
{
    [Header("장비 정보")]
    [SerializeField] private TMP_Text Text_EquipmentName;
    [SerializeField] private TMP_Text Text_Level;
    [SerializeField] private TMP_Text Text_Stat;
    [SerializeField] private TMP_Text Text_Currency;
    [SerializeField] private TMP_Text Text_EnhanceCost;
    [SerializeField] private UIButton Button_Enhance;

    [Header("장비 다음 레벨 정보")]
    [SerializeField] private TMP_Text Text_NextLevel;
    [SerializeField] private TMP_Text Text_NextStat;



    private EquipmentItem _testEquipmentData;
    private EquipmentModel _testEquipmentModel;
    private PlayerModel _testPlayerModel;

    private void OnEnable()
    {
        CreateTestData();
        RefreshUI();

        if (Button_Enhance != null)
        {
            Button_Enhance.BindOnClickButtonEvent(OnClick_Enhance);
        }
    }

    private void OnDisable()
    {
        if (Button_Enhance != null)
        {
            Button_Enhance.UnBindAllOnClickButtonEvent();
        }
    }

    private void OnClick_Enhance()
    {
        if (_testEquipmentData == null ||
            _testEquipmentModel == null ||
            _testPlayerModel == null)
        {
            return;
        }

        long enhanceCost =
            _testEquipmentModel.Level *
            50L *
            ((int)_testEquipmentData.Grade + 1);

        if (_testPlayerModel.EnhanceCurrency < enhanceCost)
        {
            Debug.Log("[성장 팝업 테스트] 강화 재화가 부족합니다.");
            return;
        }

        _testPlayerModel.EnhanceCurrency -= enhanceCost;
        _testEquipmentModel.Level++;

        RefreshUI();

        Debug.Log(
            $"[성장 팝업 테스트] 강화 성공 - " +
            $"레벨: {_testEquipmentModel.Level}, " +
            $"남은 재화: {_testPlayerModel.EnhanceCurrency}"
        );
    }

    private void RefreshUI()
    {
        if (_testEquipmentData == null ||
            _testEquipmentModel == null ||
            _testPlayerModel == null)
        {
            return;
        }

        int currentLevel = _testEquipmentModel.Level;
        int nextLevel = currentLevel + 1;

        float currentStat =
            (_testEquipmentData.BaseStatValue +
            (currentLevel - 1) *
            _testEquipmentData.StatValuePerLevel) *
            _testEquipmentData.GradeMultiplier;

        float nextStat =
            (_testEquipmentData.BaseStatValue +
            (nextLevel - 1) *
            _testEquipmentData.StatValuePerLevel) *
            _testEquipmentData.GradeMultiplier;

        float increaseStat = nextStat - currentStat;



        long enhanceCost = _testEquipmentModel.Level * 50L * ((int)_testEquipmentData.Grade + 1);

        if (Text_EquipmentName != null)
        {
            Text_EquipmentName.text = _testEquipmentData.Name;
        }

        if (Text_Level != null)
        {
            Text_Level.text = $"Lv.{_testEquipmentModel.Level}";
        }

        if (Text_Stat != null)
        {
            Text_Stat.text = $"방어력: {currentStat:0}";
        }

        if (Text_NextLevel != null)
        {
            Text_NextLevel.text = $"Lv.{nextLevel}";
        }

        if (Text_NextStat != null)
        {
            Text_NextStat.text =
                $"방어력: {nextStat:0} " +
                $"<color=#67E480>(+{increaseStat:0})</color>";
        }

        if (Text_Currency != null)
        {
            Text_Currency.text = $"강화 재화: {_testPlayerModel.EnhanceCurrency:N0}";
        }

        if (Text_EnhanceCost != null)
        {
            Text_EnhanceCost.text = $"강화 비용: {enhanceCost:N0}";
        }
    }

    private void CreateTestData()
    {
        if (_testEquipmentData != null)
        {
            return;
        }

        _testEquipmentData = new EquipmentItem
        {
            Id = "TEST_CHEST_001",
            Name = "일반 푸른 사슬갑옷",
            Type = EquipmentType.Chest,
            Grade = EquipmentGrade.Common,
            MainStatType = StatType.Defense,
            BaseStatValue = 20f,
            StatValuePerLevel = 2f,
            GradeMultiplier = 1f,
            Description = "테스트용 방어구입니다."
        };

        _testEquipmentModel = new EquipmentModel
        {
            ItemUniqueId = 1,
            ItemDataId = _testEquipmentData.Id,
            Level = 1,
            IsEquipped = true
        };

        _testPlayerModel = new PlayerModel
        {
            EnhanceCurrency = 1000
        };

        Debug.Log(
            $"[성장 팝업 테스트] " +
            $"장비: {_testEquipmentData.Name}, " +
            $"레벨: {_testEquipmentModel.Level}, " +
            $"강화 재화: {_testPlayerModel.EnhanceCurrency}"
        );
    }
}
