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

    private GameObject _overlay;
    private EquipmentCardView _currentCard;
    private EquipmentCardView _newCard;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _guide;
    private Button _equipButton;
    private TextMeshProUGUI _equipButtonText;
    private TMP_FontAsset _font;

    private EquipmentModel _selectedModel;
    private Action<EquipmentModel> _onConfirm;
    private bool _isNotice;

    public bool IsVisible => _overlay != null && _overlay.activeSelf;

    public void Show(
        EquipmentItem currentData,
        EquipmentModel currentModel,
        EquipmentItem newData,
        EquipmentModel newModel,
        Action<EquipmentModel> onConfirm)
    {
        EnsureBuilt();

        _isNotice = false;
        _onConfirm = onConfirm;
        _selectedModel = null;

        _title.text = "장비 선택";
        _guide.text =
            "장착할 장비를 선택하세요.\n" +
            "선택하지 않은 장비는 자동으로 분해됩니다.";
        _currentCard.Button.gameObject.SetActive(true);
        _newCard.Button.gameObject.SetActive(true);

        SetCard(
            _currentCard,
            "현재 장비",
            currentData,
            currentModel,
            newData,
            newModel
        );
        SetCard(
            _newCard,
            "획득 장비",
            newData,
            newModel,
            currentData,
            currentModel
        );

        _currentCard.Button.interactable =
            currentData != null && currentModel != null;
        _newCard.Button.interactable =
            newData != null && newModel != null;
        _equipButton.interactable = false;
        _equipButtonText.text = "장비를 선택해 주세요";

        RefreshSelection();
        _overlay.SetActive(true);
        _overlay.transform.SetAsLastSibling();
    }

    public void ShowNotice(string title, string message)
    {
        EnsureBuilt();

        _isNotice = true;
        _selectedModel = null;
        _onConfirm = null;

        _title.text = title;
        _guide.text = message;
        _currentCard.Button.gameObject.SetActive(false);
        _newCard.Button.gameObject.SetActive(false);
        _equipButton.interactable = true;
        _equipButtonText.text = "확인";

        _overlay.SetActive(true);
        _overlay.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (_overlay != null)
        {
            _overlay.SetActive(false);
        }

        _selectedModel = null;
        _onConfirm = null;
        _isNotice = false;
    }

    private void EnsureBuilt()
    {
        if (_overlay != null)
        {
            return;
        }

        TMP_Text existingText =
            GetComponentInChildren<TMP_Text>(true);
        _font = existingText != null
            ? existingText.font
            : TMP_Settings.defaultFontAsset;

        RectTransform overlayRect = CreateRect(
            "EquipmentChestResultOverlay",
            transform
        );
        Stretch(overlayRect);

        Image overlayImage =
            overlayRect.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform panelRect = CreateRect(
            "EquipmentChestResultPanel",
            overlayRect
        );
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 620f);

        Image panelImage =
            panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.09f, 0.08f, 0.13f, 1f);

        _title = CreateText(
            "Title",
            panelRect,
            new Vector2(0.06f, 0.87f),
            new Vector2(0.94f, 0.97f),
            25f
        );
        _title.text = "장비 선택";

        _guide = CreateText(
            "Guide",
            panelRect,
            new Vector2(0.06f, 0.78f),
            new Vector2(0.94f, 0.87f),
            15f
        );
        _guide.text =
            "장착할 장비를 선택하세요.\n" +
            "선택하지 않은 장비는 자동으로 분해됩니다.";

        _currentCard = CreateCard(
            "CurrentEquipment",
            panelRect,
            new Vector2(0.05f, 0.25f),
            new Vector2(0.49f, 0.76f)
        );
        _newCard = CreateCard(
            "NewEquipment",
            panelRect,
            new Vector2(0.51f, 0.25f),
            new Vector2(0.95f, 0.76f)
        );

        _currentCard.Button.onClick.AddListener(
            () => SelectEquipment(_currentCard.Model)
        );
        _newCard.Button.onClick.AddListener(
            () => SelectEquipment(_newCard.Model)
        );

        _equipButton = CreateButton(
            "EquipButton",
            panelRect,
            new Vector2(0.13f, 0.06f),
            new Vector2(0.87f, 0.18f),
            new Color(0.27f, 0.56f, 0.88f, 1f),
            out _equipButtonText
        );
        _equipButton.onClick.AddListener(ConfirmSelection);

        _overlay = overlayRect.gameObject;
        _overlay.SetActive(false);
    }

    private EquipmentCardView CreateCard(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform cardRect = CreateRect(objectName, parent);
        cardRect.anchorMin = anchorMin;
        cardRect.anchorMax = anchorMax;
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        Image background = cardRect.gameObject.AddComponent<Image>();
        background.color = DefaultCardColor;

        Button button = cardRect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;

        TextMeshProUGUI text = CreateText(
            "OptionText",
            cardRect,
            new Vector2(0.07f, 0.05f),
            new Vector2(0.93f, 0.95f),
            16f
        );

        return new EquipmentCardView
        {
            Button = button,
            Background = background,
            Text = text
        };
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
            card.Text.text =
                $"<b>{label}</b>\n\n" +
                "장착 중인 장비 없음";
            return;
        }

        float statValue = CalculateStat(data, model.Level);
        string statName = GetStatDisplayName(data.MainStatType);
        string comparisonText = string.Empty;

        if (comparisonData != null &&
            comparisonModel != null &&
            comparisonData.MainStatType == data.MainStatType)
        {
            float comparisonStat = CalculateStat(
                comparisonData,
                comparisonModel.Level
            );
            float difference = statValue - comparisonStat;
            string differenceColor = difference >= 0f
                ? "#67E480"
                : "#FF7777";

            comparisonText =
                $"\n<color={differenceColor}>" +
                $"비교: {difference:+0.##;-0.##;0}" +
                "</color>";
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

    private float CalculateStat(
        EquipmentItem equipmentData,
        int level)
    {
        return (
            equipmentData.BaseStatValue +
            (Mathf.Max(1, level) - 1) *
            equipmentData.StatValuePerLevel
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
            _selectedModel != null &&
            ReferenceEquals(_selectedModel, _currentCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
        _newCard.Background.color =
            _selectedModel != null &&
            ReferenceEquals(_selectedModel, _newCard.Model)
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

    private RectTransform CreateRect(
        string objectName,
        Transform parent)
    {
        GameObject gameObject = new GameObject(
            objectName,
            typeof(RectTransform)
        );
        gameObject.layer = parent.gameObject.layer;
        RectTransform rectTransform =
            gameObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        return rectTransform;
    }

    private void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize)
    {
        RectTransform rectTransform = CreateRect(
            objectName,
            parent
        );
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text =
            rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = _font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        out TextMeshProUGUI buttonText)
    {
        RectTransform rectTransform = CreateRect(
            objectName,
            parent
        );
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;

        Button button = rectTransform.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        buttonText = CreateText(
            "Text",
            rectTransform,
            Vector2.zero,
            Vector2.one,
            19f
        );
        return button;
    }

    private string GetGradeDisplayName(EquipmentGrade grade)
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

    private string GetStatDisplayName(StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack:
                return "공격력";
            case StatType.MaxHp:
                return "체력";
            case StatType.Defense:
                return "방어력";
            case StatType.CriticalChance:
                return "치명타 확률";
            case StatType.CriticalDamage:
                return "치명타 피해";
            case StatType.AttackSpeed:
                return "공격 속도";
            case StatType.MoveSpeed:
                return "이동 속도";
            case StatType.Accuracy:
                return "명중률";
            case StatType.Evasion:
                return "회피율";
            case StatType.LifeSteal:
                return "흡혈";
            case StatType.CooldownReduction:
                return "재사용 대기시간 감소";
            case StatType.GoldGainBonus:
                return "골드 획득량";
            case StatType.EquipmentDropRate:
                return "장비 획득률";
            case StatType.ExpGainBonus:
                return "경험치 획득량";
            case StatType.RebirthPointBonus:
                return "환생 포인트 획득량";
            default:
                return statType.ToString();
        }
    }

    private string GetTypeDisplayName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon:
                return "무기";
            case EquipmentType.Chest:
                return "상의";
            case EquipmentType.Pants:
                return "하의";
            case EquipmentType.Gloves:
                return "장갑";
            case EquipmentType.Boots:
                return "신발";
            case EquipmentType.Belt:
                return "벨트";
            case EquipmentType.Necklace:
                return "목걸이";
            case EquipmentType.Ring:
                return "반지";
            default:
                return type.ToString();
        }
    }
}
