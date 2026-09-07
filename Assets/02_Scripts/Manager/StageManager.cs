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
    [SerializeField] private bool _autoBossChallenge = true;

    [Header("Map Environment Settings")]
    [SerializeField] private int _stagesForChange = 10;
    [SerializeField] private BGIScroller _bgScroller;

    public int CurrentStage
    {
        get
        {
            var player = GameManager.Instance.SaveServer?.GetPlayerModel();
            return player != null ? player.CurrentStage : 1;
        }
        set
        {
            var player = GameManager.Instance.SaveServer?.GetPlayerModel();
            if (player != null)
            {
                player.CurrentStage = value;
                if (value > player.MaxStage)
                {
                    player.MaxStage = value;
                }
            }
        }
    }

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
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBattleCleared += HandleBattleCleared;
            GameManager.Instance.Combat.OnBattleFailed += HandleBattleFailed;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBattleCleared -= HandleBattleCleared;
            GameManager.Instance.Combat.OnBattleFailed -= HandleBattleFailed;
        }
    }

    public void Initialize(int stage)
    {
        CurrentStage = stage;
        InitStage(CurrentStage);
        GameManager.Instance.UI.OpenBackgroundUI(UIType.StageProgressUI);
        GameManager.Instance.UI.OpenBackgroundUI(UIType.StageInfoUI);
    }

    public void InitStage(int stageIndex)
    {
        CurrentStage = stageIndex;
        CurrentMode = StageMode.NormalStage;

        OnStageChanged?.Invoke(CurrentStage);
        OnModeChanged?.Invoke(CurrentMode);

        UpdateMainStageBGI(CurrentStage);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartNormalBattle(CurrentStage);
        }
    }

    public void StartBossChallenge()
    {
        CurrentMode = StageMode.BossStage;
        OnModeChanged?.Invoke(CurrentMode);

        GameManager.Instance.UI.OpenBackgroundUI(UIType.BossTimerUI);
        GameManager.Instance.UI.OpenBackgroundUI(UIType.BossHudUI);

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartBossBattle(CurrentStage);
        }
    }

    private void HandleBattleCleared()
    {
        if (CurrentMode == StageMode.BossStage)
        {
            GameManager.Instance.UI.CloseBackgroundUI(UIType.BossTimerUI);
            GameManager.Instance.UI.CloseBackgroundUI(UIType.BossHudUI);

            CurrentStage++;
            InitStage(CurrentStage);
        }
        else if (CurrentMode == StageMode.NormalStage)
        {
            CurrentStage++;

            if (CurrentStage % _stagesForChange == 0)
            {
                if (_autoBossChallenge)
                {
                    StartBossChallenge();
                }
                else
                {
                    CurrentStage = GameUtil.GetThemeFirstStage(CurrentStage, _stagesForChange);
                    InitStage(CurrentStage);
                }
            }
            else
            {
                OnStageChanged?.Invoke(CurrentStage);
                GameManager.Instance.Combat.StartNormalBattle(CurrentStage);
            }
        }
    }

    private void HandleBattleFailed()
    {
        GameManager.Instance.UI.CloseBackgroundUI(UIType.BossTimerUI);
        GameManager.Instance.UI.CloseBackgroundUI(UIType.BossHudUI);

        CurrentMode = StageMode.NormalStage;
        CurrentStage = GameUtil.GetThemeFirstStage(CurrentStage, _stagesForChange); // 프로퍼티 set 호출

        OnModeChanged?.Invoke(CurrentMode);
        OnStageChanged?.Invoke(CurrentStage);

        PlayerController.Instance?.ResetHp();

        if (GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.StartNormalBattle(CurrentStage);
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
