using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RebirthPopupUI : UIBase
{
    [Header("텍스트 UI")]
    [SerializeField] private TextMeshProUGUI _txtExpectedPoints;
    [SerializeField] private TextMeshProUGUI _txtConditionInfo;

    [Header("버튼 UI")]
    [SerializeField] private Button _btnRebirth;
    [SerializeField] private Button _btnClose;

    [Header("색상/연출 설정")]
    [SerializeField] private Color _enableColor = Color.white;
    [SerializeField] private Color _disableColor = Color.gray;

    private void Awake()
    {
        _btnRebirth.onClick.AddListener(OnClickRebirth);
        _btnClose.onClick.AddListener(ClosePopup);
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {

        if (GameManager.Instance == null || GameManager.Instance.Rebirth == null || GameManager.Instance.Stage == null)
        {
            Debug.LogWarning("[RebirthPopupUI] GameManager 또는 하위 매니저가 초기화되지 않았습니다.");
            return;
        }

        // 2. 인스펙터 UI 컴포넌트 null 체크
        if (_txtExpectedPoints == null || _txtConditionInfo == null)
        {
            Debug.LogWarning("[RebirthPopupUI] TextMeshProUGUI 컴포넌트 참조가 비어 있습니다.");
            return;
        }

        var gameMgr = GameManager.Instance;
        if (gameMgr == null) return;

        int currentStage = gameMgr.Stage.CurrentStage; 
        int minStage = gameMgr.Rebirth.MinRebirthStage;
        float rebirthBonus = gameMgr.Growth.GetStatValue(StatType.RebirthPointBonus);

        bool canRebirth = gameMgr.Rebirth.CanRebirth(currentStage);

        if (canRebirth)
        {
            int expectedPoints = gameMgr.Rebirth.CalculateRebirthPoints(currentStage, rebirthBonus);

            _txtExpectedPoints.gameObject.SetActive(true);
            _txtExpectedPoints.text = $"획득 가능 환생 포인트: <color=#FFD700>+{expectedPoints:N0} Point</color>";

            _txtConditionInfo.text = $"현재 <color=#00FF00>{currentStage}층</color> 달성! 환생할 준비가 되었습니다.";
            _txtConditionInfo.color = _enableColor;

            _btnRebirth.interactable = true;
        }
        else
        {
            _txtExpectedPoints.gameObject.SetActive(false);

            _txtConditionInfo.text = $"환생 불가: 최소 <color=#FF4444>{minStage}층</color> 이상 달성해야 합니다.\n(현재 달성 층수: {currentStage}층)";
            _txtConditionInfo.color = _disableColor;

            _btnRebirth.interactable = false;
        }
        Debug.Log("[UIRebirthPopup] RefreshUI 실행됨");
    }

    private void OnClickRebirth()
    {
        int currentStage = GameManager.Instance.Stage.CurrentStage;

        if (!GameManager.Instance.Rebirth.CanRebirth(currentStage))
        {
            Debug.LogWarning("환생 조건을 만족하지 못했습니다.");
            return;
        }

        ClosePopup();
        GameManager.Instance.ChangeState(GameState.Rebirth);
    }

    private void ClosePopup()
    {
        GameManager.Instance.UI.CloseRebirthPopupUI();
    }
}