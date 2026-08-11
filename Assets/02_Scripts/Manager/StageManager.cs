using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StageMode
{
    NormalStage,
    BossStage
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Progress Settings")]
    [SerializeField] private int _currentStage = 1;
    [SerializeField] private bool _autoBossChallenge = true;

    [Header("Map Environment Settings")]
    [SerializeField] private int _stagesForChangeBGI = 10;

    [SerializeField] private Image _bgImage;
    [SerializeField] private SpriteRenderer _bgSpriteRenderer;

    public int CurrentStage => _currentStage;
    public bool AutoBossChallenge
    {
        get => _autoBossChallenge;
        set => _autoBossChallenge = value;
    }

    public StageMode CurrentMode { get; private set; } = StageMode.NormalStage;

    private Dictionary<int, Sprite> _spritePool = new Dictionary<int, Sprite>();

    private int _currentThemeIndex = -1;

    public event Action<int> OnStageChanged;
    public event Action<StageMode> OnModeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnBattleCleared += HandleBattleCleared;
            CombatManager.Instance.OnBattleFailed += HandleBattleFailed;
        }

        InitStage(_currentStage);
    }

    private void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnBattleCleared -= HandleBattleCleared;
            CombatManager.Instance.OnBattleFailed -= HandleBattleFailed;
        }
    }

    public void InitStage(int stageIndex)
    {
        _currentStage = stageIndex;
        CurrentMode = StageMode.NormalStage;

        OnStageChanged?.Invoke(_currentStage);
        OnModeChanged?.Invoke(CurrentMode);

        UpdateMainStageBGI(_currentStage);

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartNormalBattle(_currentStage);
        }
    }

    public void StartBossChallenge()
    {
        CurrentMode = StageMode.BossStage;
        OnModeChanged?.Invoke(CurrentMode);

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartBossBattle(_currentStage);
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
                CombatManager.Instance.StartNormalBattle(_currentStage);
            }
        }
    }

    private void HandleBattleFailed()
    {
        CurrentMode = StageMode.NormalStage;
        OnModeChanged?.Invoke(CurrentMode);

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartNormalBattle(_currentStage);
        }
    }

    private void UpdateMainStageBGI(int stageIndex)
    {
        int targetThemeIndex = ((stageIndex - 1) / _stagesForChangeBGI) + 1;

        if (targetThemeIndex == _currentThemeIndex) return;
        if (_bgImage == null && _bgSpriteRenderer == null) return;

        if (!_spritePool.TryGetValue(targetThemeIndex, out Sprite loadedSprite))
        {
            loadedSprite = Resources.Load<Sprite>($"Sprites/Backgrounds/Theme_{targetThemeIndex}");
            if (loadedSprite == null)
            {
                Debug.LogWarning($"[StageManager] 에셋을 찾을 수 없습니다: Theme_{targetThemeIndex}");
                return;
            }
            _spritePool.Add(targetThemeIndex, loadedSprite);
        }

        _currentThemeIndex = targetThemeIndex;

        if (_bgImage != null)
        {
            _bgImage.sprite = loadedSprite;
        }
        if (_bgSpriteRenderer != null)
        {
            _bgSpriteRenderer.sprite = loadedSprite;
        }

    }
}

