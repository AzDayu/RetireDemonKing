using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum StageMode
{
    NormalStage,
    BossStage
}

public class StageManager : MonoBehaviour
{
    // 세이브 매니저 미구현, 테스트용
    [ContextMenu("테스트: 스테이지 강제 시작")]
    public void DebugForceInitStage()
    {
        Initialize(_currentStage);
    }

    [Header("Stage Progress Settings")]
    [SerializeField] private int _currentStage = 1;
    [SerializeField] private bool _autoBossChallenge = true;

    [Header("Map Environment Settings")]
    [SerializeField] private int _stagesForChange = 10;

    [SerializeField] private BGIScroller _bgScroller;

    public int CurrentStage => _currentStage;
    public int StagesForChange => _stagesForChange;

    public bool AutoBossChallenge
    {
        get => _autoBossChallenge;
        set => _autoBossChallenge = value;
    }

    public StageMode CurrentMode { get; private set; } = StageMode.NormalStage;

    private int _currentThemeIndex = -1;

    public event Action<int> OnStageChanged;
    public event Action<StageMode> OnModeChanged;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Combat.OnBattleCleared += HandleBattleCleared;
            GameManager.Instance.Combat.OnBattleFailed += HandleBattleFailed;
        }
    }

    public void Initialize(int CurrentStage)
    {
        //_currentStage = CurrentStage;
        InitStage(_currentStage);
        GameManager.Instance.UI.OpenMainUI(UIType.StageProgressUI);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBattleCleared -= HandleBattleCleared;
            GameManager.Instance.Combat.OnBattleFailed -= HandleBattleFailed;
        }
    }

    public void InitStage(int stageIndex)
    {
        _currentStage = stageIndex;
        CurrentMode = StageMode.NormalStage;

        OnStageChanged?.Invoke(_currentStage);
        OnModeChanged?.Invoke(CurrentMode);

        UpdateMainStageBGI(_currentStage);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartNormalBattle(_currentStage);
        }
    }

    public void StartBossChallenge()
    {
        CurrentMode = StageMode.BossStage;
        OnModeChanged?.Invoke(CurrentMode);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartBossBattle(_currentStage);
        }
    }

    private void HandleBattleCleared()
    {
        if (CurrentMode == StageMode.BossStage)
        {
            _currentStage++;
            InitStage(_currentStage);
        }
        else if (CurrentMode == StageMode.NormalStage)
        {
            _currentStage++;

            if (_currentStage % _stagesForChange == 0)
            {
                if (_autoBossChallenge)
                {
                    StartBossChallenge();
                }
                else
                {
                    // 자동 도전 off. 주제 첫 스테이지로 복귀
                    _currentStage = GetThemeFirstStage(_currentStage);
                    InitStage(_currentStage);
                }
            }
            else
            {
                OnStageChanged?.Invoke(_currentStage);
                GameManager.Instance.Combat.StartNormalBattle(_currentStage);
            }
        }
    }

    private int GetThemeFirstStage(int stageIndex)
    {
        int themeIndex = (int)stageIndex.GetTheme(_stagesForChange);
        return themeIndex * _stagesForChange + 1;
    }

    private void HandleBattleFailed()
    {
        CurrentMode = StageMode.NormalStage;
        _currentStage = GetThemeFirstStage(_currentStage);
        OnModeChanged?.Invoke(CurrentMode);
        OnStageChanged?.Invoke(_currentStage);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartNormalBattle(_currentStage);
        }
    }

    private async UniTaskVoid UpdateMainStageBGI(int stageIndex)
    {
        if (_bgScroller == null) return;

        int targetThemeIndex = (int)stageIndex.GetTheme(_stagesForChange);
        if (targetThemeIndex == _currentThemeIndex) return;

        string addressKey = this.GetThemeAddressKey(stageIndex, _stagesForChange);
        Sprite loadedSprite = await GameManager.Instance.Resource.LoadSprite(addressKey);

        if (loadedSprite == null)
        {
            Debug.LogWarning($"[StageManager] 에셋을 찾을 수 없습니다: {addressKey}");
            return;
        }

        _currentThemeIndex = targetThemeIndex;

        _bgScroller.SetBackgroundSprite(loadedSprite);
    }
}
