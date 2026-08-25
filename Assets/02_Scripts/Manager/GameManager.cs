using UnityEngine;

public enum GameState
{
    Init,
    Offline,
    IdleStage,
    BossChallenge,
    Rebirth,
    Pause
}

public class GameManager : MonoBehaviour
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
    [SerializeField] private GameDataManager _gameDataManager;

    public StageManager Stage => _stageManager;
    public CombatManager Combat => _combatManager;
    public GrowthManager Growth => _growthManager;
    public SaveServerManager SaveServer => _saveServerManager;
    public UIManager UI => _uiManager;
    public GameDataManager Data => _gameDataManager;

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
                if (_gameDataManager != null)
                {
                    _gameDataManager.LoadAllData();
                }
                if (_uiManager != null)
                {
                    _uiManager.OpenLoginPopupUI();
                }
                break;

            case GameState.IdleStage:
                Debug.Log("[GameManager] 방치 모드 시작");
                break;

            case GameState.BossChallenge:
                break;

            case GameState.Rebirth:
                ChangeState(GameState.IdleStage);
                break;
        }
    }

    public async void OnLoginSuccessAndStartGame()
    {
        Debug.Log("[GameManager] 로그인 성공 -> 서버/로컬 세이브 데이터 로드 시작");

        if (SaveServer != null)
        {
            bool isLoaded = await SaveServer.LoadGameDataAsync();
            if (!isLoaded)
            {
                Debug.LogError("[GameManager] 세이브 데이터를 불러오지 못했습니다.");
                return;
            }
        }

        PlayerModel playerModel = SaveServer != null ? SaveServer.GetPlayerModel() : null;
        var savedEquipments = SaveServer != null ? SaveServer.GetEquipments() : null;
        var savedRelics = SaveServer != null ? SaveServer.GetRelics() : null;

        if (_growthManager != null)
        {
            _growthManager.Initialize(playerModel, savedEquipments, savedRelics);
        }

        if (_stageManager != null && playerModel != null)
        {
            // _stageManager.Initialize(playerModel.CurrentStage);
        }

        if (_offlineManager != null && playerModel != null && SaveServer != null)
        {
            long lastSaveTicks = SaveServer.GetLastSaveUnixMinutes();
            // _offlineManager.CalculateOfflineReward(lastSaveTicks, playerModel.CurrentStage);
        }

        Debug.Log("[GameManager] 모든 초기화 완료 -> 방치 전투(IdleStage) 진입");
        ChangeState(GameState.IdleStage);
    }

    private void OnApplicationQuit()
    {
        SaveServer?.SaveGameData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveServer?.SaveGameData();
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
        Debug.Log("보스전 승리! 다음 스테이지로 이동합니다.");
        ChangeState(GameState.IdleStage);
    }
}
