using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEnhancePanelUI : UIBase
{
    [Header("스킬")]
    [SerializeField] private UIButton[] skillButtons;

    [Header("현재 레벨 스킬 정보")]
    [SerializeField] private TMP_Text CurrentSkillName;
    [SerializeField] private TMP_Text CurrentSkillText;

    [Header("다음 레벨 스킬 정보")]
    [SerializeField] private TMP_Text NextSkillName;
    [SerializeField] private TMP_Text NextSkillText;


    [Header("버튼")]
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_Enhance;
    [SerializeField] private TMP_Text EnhanceCostText;

    private readonly List<string> _skillIds = new List<string>();
    private SkillEnhanceService _skillEnhanceService;
    private int _selectedSkillIndex = -1;
    private GameObject _backgroundOverlay;

    private void Awake()
    {
        CreateBackgroundOverlay();
    }

    private void OnEnable()
    {
        SetBackgroundOverlayActive(true);

        Button_Enhance?.BindOnClickButtonEvent(
            OnClickEnhance,
            true
        );

        Button_Close?.BindOnClickButtonEvent(
            OnClickClose,
            true
        );

        InitializeSkillData();
    }

    private void OnDisable()
    {
        SetBackgroundOverlayActive(false);

        if (skillButtons != null)
        {
            foreach (UIButton button in skillButtons)
            {
                button?.UnBindAllOnClickButtonEvent();
            }
        }

        Button_Enhance?.UnBindAllOnClickButtonEvent();
        Button_Close?.UnBindAllOnClickButtonEvent();

        _skillEnhanceService = null;
        _selectedSkillIndex = -1;
    }

    private void OnDestroy()
    {
        if (_backgroundOverlay != null)
        {
            Destroy(_backgroundOverlay);
        }
    }

    private void CreateBackgroundOverlay()
    {
        if (_backgroundOverlay != null || transform.parent == null)
        {
            return;
        }

        GameObject overlay = new GameObject(
            "SkillPopupBackdrop",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        overlay.layer = gameObject.layer;

        RectTransform rectTransform = overlay.GetComponent<RectTransform>();
        rectTransform.SetParent(transform.parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.4f);

        Button button = overlay.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(OnClickClose);

        overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());
        overlay.SetActive(false);
        _backgroundOverlay = overlay;
    }

    private void SetBackgroundOverlayActive(bool isActive)
    {
        if (_backgroundOverlay == null)
        {
            CreateBackgroundOverlay();
        }

        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.SetActive(isActive);
        }
    }

    private void InitializeSkillData()
    {
        _skillIds.Clear();

        if (!SkillDataLoader.Load())
        {
            Debug.LogError("[스킬 강화] Skill.json을 불러오지 못했습니다.");
            ConfigureSkillButtons(0);
            SetEmptyState();
            return;
        }

        PlayerSaveData saveData =
            GameManager.Instance?.SaveServer?.GetSaveData();

        if (saveData == null)
        {
            Debug.LogWarning("[스킬 강화] 세이브 데이터가 없습니다.");
            ConfigureSkillButtons(0);
            SetEmptyState();
            return;
        }

        _skillEnhanceService = new SkillEnhanceService(saveData);
        _skillIds.AddRange(
            SkillDataLoader.GetSkillIdsInDisplayOrder()
        );

        ConfigureSkillButtons(_skillIds.Count);

        if (_skillIds.Count > 0)
        {
            SelectSkill(0);
        }
        else
        {
            SetEmptyState();
        }
    }

    private void ConfigureSkillButtons(int skillCount)
    {
        if (skillButtons == null)
        {
            return;
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            UIButton button = skillButtons[i];
            if (button == null)
            {
                continue;
            }

            button.UnBindAllOnClickButtonEvent();

            bool hasSkill = i < skillCount;
            button.gameObject.SetActive(hasSkill);

            if (!hasSkill)
            {
                continue;
            }

            int index = i;
            button.BindOnClickButtonEvent(
                () => SelectSkill(index),
                true
            );
        }
    }

    private void SelectSkill(int index)
    {
        if (_skillEnhanceService == null ||
            index < 0 ||
            index >= _skillIds.Count)
        {
            Debug.LogWarning(
                "[스킬 강화] 선택할 스킬 데이터가 없습니다: " +
                index
            );
            return;
        }

        _selectedSkillIndex = index;
        RefreshSelectedSkillUI();
    }

    private void RefreshSelectedSkillUI()
    {
        if (_skillEnhanceService == null ||
            _selectedSkillIndex < 0 ||
            _selectedSkillIndex >= _skillIds.Count)
        {
            SetEmptyState();
            return;
        }

        string skillId = _skillIds[_selectedSkillIndex];
        SkillItem currentData =
            _skillEnhanceService.GetCurrentData(skillId);

        if (currentData == null)
        {
            Debug.LogWarning(
                "[스킬 강화] 현재 레벨 데이터를 찾지 못했습니다: " +
                skillId
            );
            SetEmptyState();
            return;
        }

        SetText(CurrentSkillName, currentData.Name);
        SetText(CurrentSkillText, currentData.Description);
        SetText(EnhanceCostText, currentData.EnhanceCost.ToString("N0"));

        SkillItem nextData =
            _skillEnhanceService.GetNextData(skillId);

        if (nextData == null)
        {
            SetText(NextSkillName, "MAX");
            SetText(NextSkillText, "최고 레벨입니다.");

            if (Button_Enhance != null)
            {
                Button_Enhance.gameObject.SetActive(false);
            }

            return;
        }

        SetText(NextSkillName, nextData.Name);
        SetText(NextSkillText, nextData.Description);

        if (Button_Enhance != null)
        {
            Button_Enhance.gameObject.SetActive(true);
        }
    }

    private void OnClickEnhance()
    {
        if (_skillEnhanceService == null ||
            _selectedSkillIndex < 0 ||
            _selectedSkillIndex >= _skillIds.Count)
        {
            return;
        }

        string skillId = _skillIds[_selectedSkillIndex];
        SkillEnhanceResult result =
            _skillEnhanceService.TryEnhance(skillId);

        switch (result)
        {
            case SkillEnhanceResult.Success:
                GameManager.Instance?.SaveServer?.SaveGameData();
                RefreshSelectedSkillUI();
                break;

            case SkillEnhanceResult.MaxLevel:
                Debug.Log("[스킬 강화] 이미 최고 레벨입니다.");
                break;

            case SkillEnhanceResult.InsufficientCurrency:
                Debug.Log("[스킬 강화] 강화 재화가 부족합니다.");
                break;

            case SkillEnhanceResult.MissingData:
                Debug.LogWarning("[스킬 강화] 레벨 데이터를 찾지 못했습니다.");
                break;

            default:
                Debug.LogWarning("[스킬 강화] 플레이어 데이터가 없습니다.");
                break;
        }
    }

    private void OnClickClose()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.ClosePopupUI(UIType.SkillPopupUI);
        }
    }

    private void SetEmptyState()
    {
        SetText(CurrentSkillName, string.Empty);
        SetText(CurrentSkillText, string.Empty);
        SetText(NextSkillName, "MAX");
        SetText(NextSkillText, "스킬 데이터가 없습니다.");
        SetText(EnhanceCostText, "-");

        if (Button_Enhance != null)
        {
            Button_Enhance.gameObject.SetActive(false);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
