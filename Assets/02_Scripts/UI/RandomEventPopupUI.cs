using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomEventPopupUI : UIBase
{
    [Header("=== Text Fields ===")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_Choice1Desc;
    [SerializeField] private TextMeshProUGUI Text_Choice2Desc;

    [Header("=== Buttons ===")]
    [SerializeField] private Button Button_Choice1;
    [SerializeField] private Button Button_Choice2;
    [SerializeField] private Button Button_CloseSelf;

    private RandomEventStaticData _currentEventData;

    private void Awake()
    {
        InitUIButton();
    }

    private void InitUIButton()
    {
        if (Button_Choice1 != null)
        {
            Button_Choice1.onClick.RemoveAllListeners();
            Button_Choice1.onClick.AddListener(OnClickChoice1);
        }

        if (Button_Choice2 != null)
        {
            Button_Choice2.onClick.RemoveAllListeners();
            Button_Choice2.onClick.AddListener(OnClickChoice2);
        }

        if (Button_CloseSelf != null)
        {
            Button_CloseSelf.onClick.RemoveAllListeners();
            Button_CloseSelf.onClick.AddListener(OnClickClose);
        }
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
            GameManager.Instance.UI.ClosePopupUI(UIType.RandomEventPopupUI);
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