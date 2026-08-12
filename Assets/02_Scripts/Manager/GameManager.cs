using UnityEngine;

public enum GameState
{
    Init,           // 서버 로그인 및 데이터 초기화 상태
    Offline,        // 오프라인 상태
    IdleStage,      // 일반 방치 / 무한 스폰 전투 상태
    BossChallenge,  // 제한시간 보스전 진행 상태
    Rebirth,        // 환생 진행 및 리셋 연출 상태
    Pause           // 일시정지 / UI 팝업 상태
}

public class GameManager :MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== 현재 게임 상태 ===")]
    [SerializeField] private GameState _currentState = GameState.Init;
    public GameState CurrentState => _currentState;

    [Header("=== 메인 시스템 매니저 참조 ===")]
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private GrowthManager _growthManager;
    [SerializeField] private SaveServerManager _saveServerManager; 
    [SerializeField] private OfflineManager _offlineManager;
    [SerializeField] private EventManager _eventManager;
    [SerializeField] private UIManager _uiManager;

    public StageManager Stage
    {
        get
        {
            return _stageManager;
        }
    }
    public CombatManager Combat
    {
        get
        {
            return _combatManager;
        }
    }
    public GrowthManager Growth
    {
        get
        {
            return _growthManager;
        }
    }
    public SaveServerManager SaveServer
    {
        get
        {
            return _saveServerManager;
        }
    }
    public UIManager UI
    {
        get
        {
            return _uiManager;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameState.Init);
    }

    public void ChangeState(GameState newState)
    {

        OnStateExit(_currentState);
        _currentState = newState;
        OnStateEnter(_currentState);
    }

    private void OnStateExit(GameState exitState)
    {
        switch (exitState)
        {
            case GameState.IdleStage:
                break;

            case GameState.BossChallenge:
                break;
        }
    }

    private void OnStateEnter(GameState enterState)
    {
        switch (enterState)
        {
            case GameState.Init:
                InitializeGameData();
                break;

            case GameState.IdleStage:
                break;

            case GameState.BossChallenge:
                break;

            case GameState.Rebirth:
                ChangeState(GameState.IdleStage);
                break;
        }
    }

    private void InitializeGameData()
    {
        Debug.Log("게임 데이터 및 서버 로그인 처리 중");

        bool isSuccess = true;

        if (isSuccess)
        {
            ChangeState(GameState.IdleStage);
        }
        else
        {
            Debug.LogError("로그인/데이터 로드 실패!");
        }
    }

    public void OnClickBossChallenge()
    {
        if (_currentState == GameState.IdleStage)
        {
            ChangeState(GameState.BossChallenge);
        }
    }

    public void OnBossFailed()
    {
        Debug.Log("보스전 실패! 이전 스테이지 방치 모드로 돌아갑니다.");
        ChangeState(GameState.IdleStage);
    }

    public void OnBossCleared()
    {
        Debug.Log("s보스전 승리! 다음 스테이지로 이동합니다.");
        ChangeState(GameState.IdleStage);
    }
}
