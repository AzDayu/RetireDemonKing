using TMPro;
using UnityEngine;

public class RandomEventPopupUI : UIBase
{
    [Header("=== Text Fields ===")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_Choice1Desc;
    [SerializeField] private TextMeshProUGUI Text_Choice2Desc;

    [Header("=== Buttons ===")]
    [SerializeField] private UIButton Button_Choice1;
    [SerializeField] private UIButton Button_Choice2;
    [SerializeField] private UIButton Button_CloseSelf;

    private RandomEventStaticData _currentEventData;
    private bool _isInitialized = false;

    private void Awake()
    {
        InitButtonEvents();
    }

    private void InitButtonEvents()
    {
        if (_isInitialized) return;

        if (Button_Choice1 != null)
        {
            Button_Choice1.BindOnClickButtonEvent(OnClickChoice1);
        }

        if (Button_Choice2 != null)
        {
            Button_Choice2.BindOnClickButtonEvent(OnClickChoice2);
        }

        if (Button_CloseSelf != null)
        {
            Button_CloseSelf.BindOnClickButtonEvent(OnClickClose);
        }

        _isInitialized = true;
    }

    public void SetUI(RandomEventStaticData eventData)
    {
        _currentEventData = eventData;
        if (_currentEventData == null) return;

        if (Text_Title != null)
            Text_Title.text = _currentEventData.Title;

        if (Text_Description != null)
            Text_Description.text = _currentEventData.Description;

        if (Text_Choice1Desc != null)
        {
            Text_Choice1Desc.text = $"{_currentEventData.Choice1Text}\n<color=#FFD700>➔ 스테이지 골드 x{_currentEventData.GoldStageMultiplier} 획득</color>";
        }

        if (Text_Choice2Desc != null)
        {
            int durationMin = Mathf.RoundToInt(_currentEventData.BuffDurationSec / 60f);
            Text_Choice2Desc.text = $"{_currentEventData.Choice2Text}\n<color=#00FF7F>➔ {_currentEventData.BuffStatType} +{_currentEventData.BuffPercent}% ({durationMin}분 지속)</color>";
        }
    }

    private void OnClickChoice1()
    {
        if (_currentEventData != null && GameManager.Instance != null && GameManager.Instance.Event != null)
        {
            GameManager.Instance.Event.SelectChoice1_Gold(_currentEventData);
        }
        ClosePopup();
    }

    private void OnClickChoice2()
    {
        if (_currentEventData != null && GameManager.Instance != null && GameManager.Instance.Event != null)
        {
            GameManager.Instance.Event.SelectChoice2_Buff(_currentEventData);
        }
        ClosePopup();
    }

    private void OnClickClose()
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.CloseRandomEventPopupUI();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        _currentEventData = null;
    }
}
