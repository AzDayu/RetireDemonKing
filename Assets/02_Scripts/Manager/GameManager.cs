using UnityEngine;
using System;

public enum GameState
{
    Init,
    Offline,
    IdleStage,
    BossChallenge,
    Rebirth,
    Pause
}

public class GameManager :MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnGameDataReady;

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

    public GameDataManager Data
    {
        get
        {
            return _gameDataManager;
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
        Debug.Log("[GameManager] 1. 정적(Static) 데이터 로드 시작");
        if (_gameDataManager != null)
        {
            _gameDataManager.LoadAllData();
            OnGameDataReady?.Invoke();  // 데이터 로드 마침 알림
        }
        else
        {
            Debug.LogError("[GameManager] GameDataManager 참조가 누락되었습니다!");
            return;
        }

        Debug.Log("[GameManager] 2. 유저 세이브 데이터 로드 시작");
        PlayerModel loadedPlayerModel = null;
        var savedEquipments = new System.Collections.Generic.List<EquipmentModel>();
        var savedRelics = new System.Collections.Generic.List<RelicModel>();

        if (_saveServerManager != null)
        {
            // 세이브/서버 매니저를 통해 데이터를 읽어옴 (없을 경우 새로 생성)
            // loadedPlayerModel = _saveServerManager.LoadPlayerData();
            // savedEquipments = _saveServerManager.LoadEquipmentData();
            // savedRelics = _saveServerManager.LoadRelicData();
        }

        if (loadedPlayerModel == null)
        {
            loadedPlayerModel = new PlayerModel
            {
                Level = 1,
                CurrentExp = 0,
                Gold = 0,
                EnhanceCurrency = 0,
                RebirthPoints = 0,
                CurrentStage = 1,
                MaxStage = 1
            };
        }

        Debug.Log("[GameManager] 3. 성장 매니저(GrowthManager) 초기화 및 스탯 계산");
        if (_growthManager != null)
        {
            _growthManager.Initialize(loadedPlayerModel, savedEquipments, savedRelics);
        }

        Debug.Log("[GameManager] 4. 스테이지 매니저(StageManager) 초기화");
        if (_stageManager != null)
        {
            //_stageManager.Initialize(loadedPlayerModel.CurrentStage);
        }

        Debug.Log("[GameManager] 5. UI 및 오프라인 보상 처리");
        if (_uiManager != null)
        {
            //_uiManager.Initialize();
        }

        // 오프라인 보상이 있다면 계산 후 팝업 띄우기
        // _offlineManager?.CalculateOfflineReward();

        Debug.Log("[GameManager] 모든 초기화 완료! 방치 스테이지 진입");
        ChangeState(GameState.IdleStage);
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
