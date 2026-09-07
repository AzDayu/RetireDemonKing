using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentChestResultPanelUI : UIBase
{
    private const float PreferredPanelScale = 1.5f;
    private const float MinimumPanelScale = 0.75f;
    private const float HorizontalSafePadding = 64f;
    private const float VerticalSafePadding = 96f;

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
    private EquipmentCardView _droppedRingCard;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _guide;
    private Button _equipButton;
    private TextMeshProUGUI _equipButtonText;
    private Button _currentEquipButton;
    private Button _newEquipButton;
    private RectTransform _panelRect;
    private Vector2 _defaultPanelSize;

    private EquipmentModel _selectedModel;
    private Action<EquipmentModel> _onConfirm;
    private Action<IReadOnlyList<EquipmentModel>> _onRingConfirm;
    private readonly List<EquipmentModel> _selectedRings =
        new List<EquipmentModel>();
    private bool _useImmediateEquipButtons;
    private bool _isRingSelection;
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

        ResetSelectionState();
        _onConfirm = onConfirm;

        _title.text = "장비 선택";
        _guide.text =
            "장착할 장비를 선택하세요.\n" +
            "선택하지 않은 장비는 자동으로 분해됩니다.";
        SetSingleEquipmentLayout(false);

        SetCard(_currentCard, "현재 장비", currentData, currentModel, newData, newModel);
        SetCard(_newCard, "드랍 장비", newData, newModel, currentData, currentModel);

        _currentCard.Button.interactable = currentData != null && currentModel != null;
        _newCard.Button.interactable = newData != null && newModel != null;
        _equipButton.interactable = false;
        _equipButtonText.text = "장비를 선택해 주세요";

        RefreshSelection();
        ShowPanel();
    }

    public void ShowDropSelection(
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

        ResetSelectionState();
        _useImmediateEquipButtons = true;
        _onConfirm = onConfirm;

        _title.text = "드랍 장비 선택";
        _guide.text =
            "장착할 장비의 착용 버튼을 누르세요.\n" +
            "선택하지 않은 장비는 자동으로 분해됩니다.";
        SetSingleEquipmentLayout(true);

        SetCard(_currentCard, "현재 장비", currentData, currentModel, newData, newModel);
        SetCard(_newCard, "드랍 장비", newData, newModel, currentData, currentModel);

        _currentEquipButton.interactable = currentData != null && currentModel != null;
        _newEquipButton.interactable = newData != null && newModel != null;
        RefreshSelection();
        ShowPanel();
    }

    public void ShowRingSelection(
        EquipmentModel firstRing,
        EquipmentModel secondRing,
        EquipmentModel droppedRing,
        Action<IReadOnlyList<EquipmentModel>> onConfirm)
    {
        if (!Initialize())
        {
            return;
        }

        ResetSelectionState();
        _isRingSelection = true;
        _onRingConfirm = onConfirm;

        _title.text = "반지 선택";
        _guide.text =
            "장착할 반지 두 개를 선택한 뒤 확인을 누르세요.\n" +
            "선택하지 않은 반지는 자동으로 분해됩니다.";
        SetRingSelectionLayout();

        SetCardFromModel(_currentCard, "반지 1", firstRing);
        SetCardFromModel(_newCard, "반지 2", secondRing);
        SetCardFromModel(_droppedRingCard, "드랍 반지", droppedRing);

        RefreshSelection();
        RefreshRingConfirmButton();
        ShowPanel();
    }

    public void ShowNotice(string title, string message)
    {
        if (!Initialize())
        {
            return;
        }

        ResetSelectionState();
        _isNotice = true;

        _title.text = title;
        _guide.text = message;
        _panelRect.sizeDelta = _defaultPanelSize;
        _currentCard.Button.gameObject.SetActive(false);
        _newCard.Button.gameObject.SetActive(false);
        _droppedRingCard.Button.gameObject.SetActive(false);
        _currentEquipButton.gameObject.SetActive(false);
        _newEquipButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
        _equipButton.interactable = true;
        _equipButtonText.text = "확인";

        ShowPanel();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ResetSelectionState();
    }

    private bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        _panelRect = transform.Find("Panel") as RectTransform;
        _title = FindText("Panel/Title");
        _guide = FindText("Panel/Guide");
        _currentCard = FindCard("Panel/CurrentEquipment");
        _newCard = FindCard("Panel/NewEquipment");
        _equipButton = FindButton("Panel/EquipButton");
        _equipButtonText = FindText("Panel/EquipButton/Text");

        if (_panelRect == null || _title == null || _guide == null || _currentCard == null ||
            _newCard == null || _equipButton == null || _equipButtonText == null)
        {
            Debug.LogError("[EquipmentChestResultPanelUI] 결과창 프리팹 참조를 찾지 못했습니다.");
            return false;
        }

        _defaultPanelSize = _panelRect.sizeDelta;
        _droppedRingCard = CreateDroppedRingCard();
        _currentEquipButton = CreateChoiceButton(
            "CurrentEquipButton",
            new Vector2(0.05f, 0.06f),
            new Vector2(0.49f, 0.18f)
        );
        _newEquipButton = CreateChoiceButton(
            "NewEquipButton",
            new Vector2(0.51f, 0.06f),
            new Vector2(0.95f, 0.18f)
        );

        if (_droppedRingCard == null || _currentEquipButton == null ||
            _newEquipButton == null)
        {
            Debug.LogError("[EquipmentChestResultPanelUI] 동적 선택 UI를 생성하지 못했습니다.");
            return false;
        }

        SetDefaultFont(_title);
        SetDefaultFont(_guide);
        SetDefaultFont(_currentCard.Text);
        SetDefaultFont(_newCard.Text);
        SetDefaultFont(_droppedRingCard.Text);
        SetDefaultFont(_equipButtonText);

        _currentCard.Button.onClick.AddListener(() => SelectEquipment(_currentCard.Model));
        _newCard.Button.onClick.AddListener(() => SelectEquipment(_newCard.Model));
        _droppedRingCard.Button.onClick.AddListener(() => SelectEquipment(_droppedRingCard.Model));
        _currentEquipButton.onClick.AddListener(() => ConfirmEquipment(_currentCard.Model));
        _newEquipButton.onClick.AddListener(() => ConfirmEquipment(_newCard.Model));
        _equipButton.onClick.AddListener(ConfirmSelection);

        _isInitialized = true;
        gameObject.SetActive(false);
        return true;
    }

    private EquipmentCardView CreateDroppedRingCard()
    {
        GameObject cardObject = Instantiate(
            _newCard.Button.gameObject,
            _newCard.Button.transform.parent,
            false
        );
        cardObject.name = "DroppedRing";

        Button button = cardObject.GetComponent<Button>();
        Image background = cardObject.GetComponent<Image>();
        Transform textTransform = cardObject.transform.Find("OptionText");
        TextMeshProUGUI text = textTransform != null
            ? textTransform.GetComponent<TextMeshProUGUI>()
            : null;

        if (button == null || background == null || text == null)
        {
            Destroy(cardObject);
            return null;
        }

        return new EquipmentCardView
        {
            Button = button,
            Background = background,
            Text = text
        };
    }

    private Button CreateChoiceButton(
        string buttonName,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject buttonObject = Instantiate(
            _equipButton.gameObject,
            _equipButton.transform.parent,
            false
        );
        buttonObject.name = buttonName;

        RectTransform rectTransform =
            buttonObject.GetComponent<RectTransform>();
        Button button = buttonObject.GetComponent<Button>();
        Transform textTransform = buttonObject.transform.Find("Text");
        TextMeshProUGUI text = textTransform != null
            ? textTransform.GetComponent<TextMeshProUGUI>()
            : null;

        if (rectTransform == null || button == null || text == null)
        {
            Destroy(buttonObject);
            return null;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        text.text = "착용";
        SetDefaultFont(text);
        return button;
    }

    private void SetSingleEquipmentLayout(bool useImmediateEquipButtons)
    {
        _panelRect.sizeDelta = _defaultPanelSize;
        SetCardAnchors(
            _currentCard,
            new Vector2(0.05f, 0.25f),
            new Vector2(0.49f, 0.76f)
        );
        SetCardAnchors(
            _newCard,
            new Vector2(0.51f, 0.25f),
            new Vector2(0.95f, 0.76f)
        );

        _currentCard.Button.gameObject.SetActive(true);
        _newCard.Button.gameObject.SetActive(true);
        _droppedRingCard.Button.gameObject.SetActive(false);
        _currentEquipButton.gameObject.SetActive(useImmediateEquipButtons);
        _newEquipButton.gameObject.SetActive(useImmediateEquipButtons);
        _equipButton.gameObject.SetActive(!useImmediateEquipButtons);
    }

    private void SetRingSelectionLayout()
    {
        _panelRect.sizeDelta = new Vector2(
            Mathf.Max(650f, _defaultPanelSize.x),
            _defaultPanelSize.y
        );
        SetCardAnchors(
            _currentCard,
            new Vector2(0.03f, 0.25f),
            new Vector2(0.32f, 0.76f)
        );
        SetCardAnchors(
            _newCard,
            new Vector2(0.355f, 0.25f),
            new Vector2(0.645f, 0.76f)
        );
        SetCardAnchors(
            _droppedRingCard,
            new Vector2(0.68f, 0.25f),
            new Vector2(0.97f, 0.76f)
        );

        _currentCard.Button.gameObject.SetActive(true);
        _newCard.Button.gameObject.SetActive(true);
        _droppedRingCard.Button.gameObject.SetActive(true);
        _currentEquipButton.gameObject.SetActive(false);
        _newEquipButton.gameObject.SetActive(false);
        _equipButton.gameObject.SetActive(true);
    }

    private static void SetCardAnchors(
        EquipmentCardView card,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        RectTransform rectTransform =
            card.Button.transform as RectTransform;
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
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
        if (text != null && text.font == null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }
    }

    private void ShowPanel()
    {
        gameObject.SetActive(true);
        ApplyResponsivePanelScale();
        transform.SetAsLastSibling();
    }

    private void ApplyResponsivePanelScale()
    {
        if (_panelRect == null)
            return;

        RectTransform viewport = transform as RectTransform;
        if (viewport == null || viewport.rect.width <= 0f || viewport.rect.height <= 0f)
        {
            _panelRect.localScale = new Vector3(PreferredPanelScale, PreferredPanelScale, 1f);
            return;
        }

        float panelWidth = Mathf.Max(1f, _panelRect.rect.width);
        float panelHeight = Mathf.Max(1f, _panelRect.rect.height);
        float widthScale = Mathf.Max(0f, viewport.rect.width - HorizontalSafePadding) / panelWidth;
        float heightScale = Mathf.Max(0f, viewport.rect.height - VerticalSafePadding) / panelHeight;
        float availableScale = Mathf.Min(PreferredPanelScale, Mathf.Min(widthScale, heightScale));
        float scale = Mathf.Clamp(availableScale, MinimumPanelScale, PreferredPanelScale);

        _panelRect.localScale = new Vector3(scale, scale, 1f);
    }

    private void SetCardFromModel(
        EquipmentCardView card,
        string label,
        EquipmentModel model)
    {
        EquipmentItem data = model != null
            ? GameManager.Instance?.Data?.GetEquipmentData(model.ItemDataId)
            : null;
        SetCard(card, label, data, model, null, null);
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
            card.Button.interactable = false;
            return;
        }

        card.Button.interactable = true;

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

        if (_isRingSelection)
        {
            if (_selectedRings.Contains(equipmentModel))
            {
                _selectedRings.Remove(equipmentModel);
            }
            else if (_selectedRings.Count < 2)
            {
                _selectedRings.Add(equipmentModel);
            }

            RefreshSelection();
            RefreshRingConfirmButton();
            return;
        }

        _selectedModel = equipmentModel;
        if (!_useImmediateEquipButtons)
        {
            _equipButton.interactable = true;
            _equipButtonText.text = "장착";
        }
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        _currentCard.Background.color =
            IsSelected(_currentCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
        _newCard.Background.color =
            IsSelected(_newCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
        _droppedRingCard.Background.color =
            IsSelected(_droppedRingCard.Model)
                ? SelectedCardColor
                : DefaultCardColor;
    }

    private bool IsSelected(EquipmentModel equipmentModel)
    {
        if (equipmentModel == null)
        {
            return false;
        }

        return _isRingSelection
            ? _selectedRings.Contains(equipmentModel)
            : _selectedModel != null &&
              ReferenceEquals(_selectedModel, equipmentModel);
    }

    private void RefreshRingConfirmButton()
    {
        _equipButton.interactable = _selectedRings.Count == 2;
        _equipButtonText.text = _selectedRings.Count == 2
            ? "선택한 반지 장착"
            : $"반지 선택 ({_selectedRings.Count}/2)";
    }

    private void ConfirmEquipment(EquipmentModel equipmentModel)
    {
        if (_isNotice || _isRingSelection || !_useImmediateEquipButtons ||
            equipmentModel == null ||
            _onConfirm == null)
        {
            return;
        }

        Action<EquipmentModel> onConfirm = _onConfirm;
        Hide();
        onConfirm.Invoke(equipmentModel);
    }

    private void ConfirmSelection()
    {
        if (_isNotice)
        {
            Hide();
            return;
        }

        if (_isRingSelection)
        {
            if (_selectedRings.Count != 2 || _onRingConfirm == null)
            {
                return;
            }

            Action<IReadOnlyList<EquipmentModel>> onRingConfirm =
                _onRingConfirm;
            var selectedRings =
                new List<EquipmentModel>(_selectedRings);
            Hide();
            onRingConfirm.Invoke(selectedRings);
            return;
        }

        if (_selectedModel == null || _onConfirm == null)
        {
            return;
        }

        EquipmentModel selectedModel = _selectedModel;
        Action<EquipmentModel> onConfirm = _onConfirm;
        Hide();
        onConfirm.Invoke(selectedModel);
    }

    private void ResetSelectionState()
    {
        _selectedModel = null;
        _selectedRings.Clear();
        _onConfirm = null;
        _onRingConfirm = null;
        _useImmediateEquipButtons = false;
        _isRingSelection = false;
        _isNotice = false;

        if (_currentCard != null)
        {
            _currentCard.Model = null;
        }
        if (_newCard != null)
        {
            _newCard.Model = null;
        }
        if (_droppedRingCard != null)
        {
            _droppedRingCard.Model = null;
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
