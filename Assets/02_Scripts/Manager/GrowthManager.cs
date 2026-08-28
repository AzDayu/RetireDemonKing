using System;
using System.Collections.Generic;
using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    [SerializeField] private EquipmentManager _equipmentManager;
    [SerializeField] private RelicManager _relicManager;

    private const long InitialEnhanceCurrency = 20000;

    private PlayerModel _playerModel = new PlayerModel
    {
        EnhanceCurrency = InitialEnhanceCurrency
    };

    public PlayerModel PlayerModel => _playerModel;

    private StatCalculator _calculator = new StatCalculator();
    private Dictionary<StatType, float> _cachedFinalStats = new Dictionary<StatType, float>();

    public event Action OnStatsUpdated;
    public event Action<int> OnLevelUpdated;

    public void Initialize(PlayerModel playerModel, List<EquipmentModel> savedEquipment = null, List<RelicModel> savedRelics = null)
    {
        _playerModel = playerModel ?? _playerModel;

        ApplyLevelBaseStats();

        _equipmentManager?.Initialize(savedEquipment);
        _relicManager?.Initialize(savedRelics);

        RecalculateTotalStats();
    }

    private void ApplyLevelBaseStats()
    {
        int level = _playerModel.Level;

        float baseAtk = 100f + ((level - 1) * 15f);
        float baseHp = 500f + ((level - 1) * 60f);
        float baseDef = 10f + ((level - 1) * 3f);

        _calculator.SetBaseStat(StatType.Attack, baseAtk);
        _calculator.SetBaseStat(StatType.MaxHp, baseHp);
        _calculator.SetBaseStat(StatType.Defense, baseDef);
        _calculator.SetBaseStat(StatType.AttackSpeed, 1f);
        _calculator.SetBaseStat(StatType.CriticalDamage, 100f);
        _calculator.SetBaseStat(StatType.Accuracy, 100f);
        _calculator.SetBaseStat(StatType.MoveSpeed, 5f);
    }

    public void AddExp(long amount)
    {
        float expBonus = GetStat(StatType.ExpGainBonus);
        long finalExp = Mathf.RoundToInt(amount * (1f + (expBonus / 100f)));

        _playerModel.CurrentExp += finalExp;

        long requiredExp = GetRequiredExp(_playerModel.Level);

        bool isLevelUp = false;
        while (_playerModel.CurrentExp >= requiredExp)
        {
            _playerModel.CurrentExp -= requiredExp;
            _playerModel.Level++;
            isLevelUp = true;

            requiredExp = GetRequiredExp(_playerModel.Level);
        }

        if (isLevelUp)
        {
            Debug.Log($"[GrowthManager] 용사 레벨업! 현재 레벨: {_playerModel.Level}");

            ApplyLevelBaseStats();
            RecalculateTotalStats();

            OnLevelUpdated?.Invoke(_playerModel.Level);
        }
    }

    public void RecalculateTotalStats()
    {
        Dictionary<StatType, float> flatBonuses = _equipmentManager != null ? _equipmentManager.GetTotalFlatStats() : null;
        Dictionary<StatType, float> percentBonuses = _relicManager != null ? _relicManager.GetTotalPercentStats() : null;

        _cachedFinalStats = _calculator.CalculateAllStats(flatBonuses, percentBonuses);

        Debug.Log($"[GrowthManager] 최종 스탯 갱신 - ATK: {GetStat(StatType.Attack)}, HP: {GetStat(StatType.MaxHp)}");

        OnStatsUpdated?.Invoke();
    }

    public float GetStat(StatType statType)
    {
        return _cachedFinalStats.TryGetValue(statType, out float value) ? value : 0f;
    }

    private long GetRequiredExp(int level)
    {
        return level * 100L;
    }

    public int CurrentLevel => _playerModel != null ? _playerModel.Level : 1;
}