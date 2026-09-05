using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentChestResultPanelUI : MonoBehaviour
{
    private static readonly Color DefaultCardColor =
        new Color(0.16f, 0.14f, 0.2f, 1f);
    private static readonly Color SelectedCardColor =
        new Color(0.23f, 0.47f, 0.76f, 1f);

    private sealed class EquipmentCardView
    {
        public Button Button;
        public Image Background;
        public TextMeshProUGUI Text;
        public EquipmentModel Model;
    }

    private EquipmentCardView _currentCard;
    private EquipmentCardView _newCard;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _guide;
    private Button _equipButton;
    private TextMeshProUGUI _equipButtonText;

    private EquipmentModel _selectedModel;
    private Action<EquipmentModel> _onConfirm;
    private bool _isNotice;
    private bool _isInitialized;

    public bool IsVisible => gameObject.activeSelf;

    private void Awake()
    {
        Initialize();
    }

    public void Show(
        EquipmentItem currentData,
        EquipmentModel currentModel,
        EquipmentItem newData,
        EquipmentModel newModel,
        Action<EquipmentModel> onConfirm)
    {
        if (!Initialize())
        {
            return;
        }

        _isNotice = false;
        _onConfirm = onConfirm;
        _selectedModel = null;

        _title.text = "장비 선택";
        _guide.text =
            "장착할 장비를 선택하세요.\n" +
            "선택하지 않은 장비는 자동으로 분해됩니다.";
        _currentCard.Button.gameObject.SetActive(true);
        _newCard.Button.gameObject.SetActive(true);

        SetCard(_currentCard, "현재 장비", currentData, currentModel, newData, newModel);
        SetCard(_newCard, "획득 장비", newData, newModel, currentData, currentModel);

        _currentCard.Button.interactable = currentData != null && currentModel != null;
        _newCard.Button.interactable = newData != null && newModel != null;
        _equipButton.interactable = false;
        _equipButtonText.text = "장비를 선택해 주세요";

        RefreshSelection();
        ShowPanel();
    }

    public void ShowNotice(string title, string message)
    {
        if (!Initialize())
        {
            return;
        }

        _isNotice = true;
        _selectedModel = null;
        _onConfirm = null;

        _title.text = title;
        _guide.text = message;
        _currentCard.Button.gameObject.SetActive(false);
        _newCard.Button.gameObject.SetActive(false);
        _equipButton.interactable = true;
        _equipButtonText.text = "확인";

        ShowPanel();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _selectedModel = null;
        _onConfirm = null;
        _isNotice = false;
    }

    private bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        _title = FindText("Panel/Title");
        _guide = FindText("Panel/Guide");
        _currentCard = FindCard("Panel/CurrentEquipment");
        _newCard = FindCard("Panel/NewEquipment");
        _equipButton = FindButton("Panel/EquipButton");
        _equipButtonText = FindText("Panel/EquipButton/Text");

        if (_title == null || _guide == null || _currentCard == null ||
            _newCard == null || _equipButton == null || _equipButtonText == null)
        {
            Debug.LogError("[EquipmentChestResultPanelUI] 결과창 프리팹 참조를 찾지 못했습니다.");
            return false;
        }

        SetDefaultFont(_title);
        SetDefaultFont(_guide);
        SetDefaultFont(_currentCard.Text);
        SetDefaultFont(_newCard.Text);
        SetDefaultFont(_equipButtonText);

        _currentCard.Button.onClick.AddListener(() => SelectEquipment(_currentCard.Model));
        _newCard.Button.onClick.AddListener(() => SelectEquipment(_newCard.Model));
        _equipButton.onClick.AddListener(ConfirmSelection);

        _isInitialized = true;
        gameObject.SetActive(false);
        return true;
    }

    private EquipmentCardView FindCard(string path)
    {
        Transform cardTransform = transform.Find(path);

        if (cardTransform == null)
        {
            return null;
        }

        Button button = cardTransform.GetComponent<Button>();
        Image background = cardTransform.GetComponent<Image>();
        TextMeshProUGUI text = FindText($"{path}/OptionText");

        if (button == null || background == null || text == null)
        {
            return null;
        }

        return new EquipmentCardView
        {
            Button = button,
            Background = background,
            Text = text
        };
    }

    private Button FindButton(string path)
    {
        Transform buttonTransform = transform.Find(path);
        return buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI FindText(string path)
    {
        Transform textTransform = transform.Find(path);
        return textTransform != null ? textTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private void SetDefaultFont(TextMeshProUGUI text)
    {
        if (text.font == null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }
    }

    private void ShowPanel()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void SetCard(
        EquipmentCardView card,
        string label,
        EquipmentItem data,
        EquipmentModel model,
        EquipmentItem comparisonData,
        EquipmentModel comparisonModel)
    {
        card.Model = model;

        if (data == null || model == null)
        {
            card.Text.text = $"<b>{label}</b>\n\n장착 중인 장비 없음";
            return;
        }

        float statValue = CalculateStat(data, model.Level);
        string statName = GetStatDisplayName(data.MainStatType);
        string comparisonText = string.Empty;

        if (comparisonData != null && comparisonModel != null &&
            comparisonData.MainStatType == data.MainStatType)
        {
            float comparisonStat = CalculateStat(comparisonData, comparisonModel.Level);
            float difference = statValue - comparisonStat;
            string differenceColor = difference >= 0f ? "#67E480" : "#FF7777";
            comparisonText = $"\n<color={differenceColor}>비교: {difference:+0.##;-0.##;0}</color>";
        }

        card.Text.text =
            $"<b>{label}</b>\n\n" +
            $"[{GetGradeDisplayName(data.Grade)}]\n" +
            $"{data.Name}\n\n" +
            $"부위: {GetTypeDisplayName(data.Type)}\n" +
            $"레벨: {model.Level}\n" +
            $"{statName}: {statValue:0.##}" +
            comparisonText;
    }

    private float CalculateStat(EquipmentItem equipmentData, int level)
    {
        return (
            equipmentData.BaseStatValue +
            (Mathf.Max(1, level) - 1) * equipmentData.StatValuePerLevel
        ) * equipmentData.GradeMultiplier;
    }

    private void SelectEquipment(EquipmentModel equipmentModel)
    {
        if (equipmentModel == null)
        {
            return;
        }

        _selectedModel = equipmentModel;
        _equipButton.interactable = true;
        _equipButtonText.text = "장착";
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        _currentCard.Background.color =
            _selectedModel != null && ReferenceEquals(_selectedModel, _currentCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
        _newCard.Background.color =
            _selectedModel != null && ReferenceEquals(_selectedModel, _newCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
    }

    private void ConfirmSelection()
    {
        if (_isNotice)
        {
            Hide();
            return;
        }

        if (_selectedModel == null || _onConfirm == null)
        {
            return;
        }

        EquipmentModel selectedModel = _selectedModel;
        Action<EquipmentModel> onConfirm = _onConfirm;
        _equipButton.interactable = false;

        try
        {
            onConfirm.Invoke(selectedModel);
        }
        finally
        {
            Hide();
        }
    }

    private string GetGradeDisplayName(EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.Common: return "일반";
            case EquipmentGrade.Rare: return "고급";
            case EquipmentGrade.Epic: return "희귀";
            case EquipmentGrade.Legendary: return "전설";
            case EquipmentGrade.Mythic: return "신화";
            default: return grade.ToString();
        }
    }

    private string GetStatDisplayName(StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack: return "공격력";
            case StatType.MaxHp: return "체력";
            case StatType.Defense: return "방어력";
            case StatType.CriticalChance: return "치명타 확률";
            case StatType.CriticalDamage: return "치명타 피해";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.MoveSpeed: return "이동 속도";
            case StatType.Accuracy: return "명중률";
            case StatType.Evasion: return "회피율";
            case StatType.LifeSteal: return "흡혈";
            case StatType.CooldownReduction: return "재사용 대기시간 감소";
            case StatType.GoldGainBonus: return "골드 획득량";
            case StatType.EquipmentDropRate: return "장비 획득률";
            case StatType.ExpGainBonus: return "경험치 획득량";
            case StatType.RebirthPointBonus: return "환생 포인트 획득량";
            default: return statType.ToString();
        }
    }

    private string GetTypeDisplayName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon: return "무기";
            case EquipmentType.Chest: return "상의";
            case EquipmentType.Pants: return "하의";
            case EquipmentType.Gloves: return "장갑";
            case EquipmentType.Boots: return "신발";
            case EquipmentType.Belt: return "벨트";
            case EquipmentType.Necklace: return "목걸이";
            case EquipmentType.Ring: return "반지";
            default: return type.ToString();
        }
    }
}
