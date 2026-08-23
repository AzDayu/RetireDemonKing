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
    [Header("Stage Progress Settings")]
    [SerializeField] private int _currentStage = 1;
    [SerializeField] private bool _autoBossChallenge = true;

    [Header("Map Environment Settings")]
    [SerializeField] private int _stagesForChangeBGI = 10;

    [SerializeField] private BGIScroller _bgScroller;

    public int CurrentStage => _currentStage;
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
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBattleCleared += HandleBattleCleared;
            GameManager.Instance.Combat.OnBattleFailed += HandleBattleFailed;
        }

        InitStage(_currentStage);
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
            // 보스 처치 성공 -> 다음 스테이지로 진행
            _currentStage++;
            InitStage(_currentStage);
        }
        else if (CurrentMode == StageMode.NormalStage)
        {
            // 일반 몬스터 목표 수량 달성시
            if (_autoBossChallenge)
            {
                StartBossChallenge();
            }
            else
            {
                // 자동 도전 off 일반 사냥 계속 진행
                GameManager.Instance.Combat.StartNormalBattle(_currentStage);
            }
        }
    }

    private void HandleBattleFailed()
    {
        CurrentMode = StageMode.NormalStage;
        OnModeChanged?.Invoke(CurrentMode);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartNormalBattle(_currentStage);
        }
    }

    private async UniTaskVoid UpdateMainStageBGI(int stageIndex)
    {
        if (_bgScroller == null) return;

        int targetThemeIndex = ((stageIndex - 1) / _stagesForChangeBGI);
        if (targetThemeIndex == _currentThemeIndex) return;

        string addressKey = this.GetThemeAddressKey(stageIndex, _stagesForChangeBGI);
        Sprite loadedSprite = await ResourceManager.Inst.LoadSprite(addressKey);

        if (loadedSprite == null)
        {
            Debug.LogWarning($"[StageManager] 에셋을 찾을 수 없습니다: {addressKey}");
            return;
        }

        _currentThemeIndex = targetThemeIndex;

        _bgScroller.SetBackgroundSprite(loadedSprite);
    }
}
