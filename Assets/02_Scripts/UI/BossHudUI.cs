using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHudUI : UIBase
{
    [SerializeField] private Slider _hpBar_Slider;

    private CombatManager _combatManager;
    private MonsterViewModel _viewModel;

    private void Awake()
    {
        InitCombatManager();
        _hpBar_Slider.gameObject.SetActive(false);
    }

    private void InitCombatManager()
    {
        _combatManager = GameManager.Instance.Combat;
    }

    private void OnEnable()
    {
        _combatManager.OnBossSpawned += HandleBossSpawned;
        _combatManager.OnBattleCleared += HandleBattleEnded;
        _combatManager.OnBattleFailed += HandleBattleEnded;
    }

    private void OnDisable()
    {
        _combatManager.OnBossSpawned -= HandleBossSpawned;
        _combatManager.OnBattleCleared -= HandleBattleEnded;
        _combatManager.OnBattleFailed -= HandleBattleEnded;
        HandleBattleEnded();
    }

    private void HandleBossSpawned(MonsterController controller)
    {
        _viewModel = new MonsterViewModel(controller.Model);
        _viewModel.PropertyChanged += HandlePropertyChanged;
        _viewModel.InvokeOnceOnInit();
        _hpBar_Slider.gameObject.SetActive(true);
    }

    private void HandleBattleEnded()
    {
        if (_viewModel == null) return;

        _viewModel.PropertyChanged -= HandlePropertyChanged;
        _viewModel.Dispose();
        _viewModel = null;
        _hpBar_Slider.gameObject.SetActive(false);
    }

    private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MonsterViewModel.HpRatio))
        {
            _hpBar_Slider.value = _viewModel.HpRatio;
        }
    }
}