using System;
using System.Collections.Generic;
using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    [Header("=== Sub Managers ===")]
    [SerializeField] private EquipmentManager _equipmentManager;
    [SerializeField] private RelicManager _relicManager;

    public EquipmentManager Equipment => _equipmentManager;
    public RelicManager Relic => _relicManager;

    public bool IsInitialized { get; private set; } = false;

    private PlayerModel _playerModel;
    private StatCalculator _calculator = new StatCalculator();
    private Dictionary<StatType, float> _cachedFinalStats = new Dictionary<StatType, float>();

    public event Action OnStatsUpdated;
    public event Action<int> OnLevelUpdated;

    public void Initialize(PlayerModel playerModel, List<EquipmentModel> savedEquipment = null, List<RelicModel> savedRelics = null)
    {
        IsInitialized = false;

        _playerModel = playerModel ?? new PlayerModel();

        ApplyLevelBaseStats();

        _equipmentManager?.Initialize(savedEquipment);
        _relicManager?.Initialize(savedRelics);

        IsInitialized = true;

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

        _calculator.SetBaseStat(StatType.CriticalDamage, 100f);
        _calculator.SetBaseStat(StatType.Accuracy, 100f);
        _calculator.SetBaseStat(StatType.MoveSpeed, 5f);
        _calculator.SetBaseStat(StatType.AttackSpeed, 1f);
    }

    public void RecalculateTotalStats()
    {
        Dictionary<StatType, float> flatBonuses = _equipmentManager != null ? _equipmentManager.GetTotalFlatStats() : null;
        Dictionary<StatType, float> percentBonuses = _relicManager != null ? _relicManager.GetTotalPercentStats() : null;

        _cachedFinalStats = _calculator.CalculateAllStats(flatBonuses, percentBonuses);

        Debug.Log($"[GrowthManager] 최종 스탯 갱신 - ATK: {GetStatValue(StatType.Attack)}, HP: {GetStatValue(StatType.MaxHp)}");

        OnStatsUpdated?.Invoke();
    }

    public float GetStatValue(StatType statType)
    {
        if (!IsInitialized) return 0f;

        return _cachedFinalStats.TryGetValue(statType, out float value) ? value : 0f;
    }

    public void AddExp(long amount)
    {
        float expBonus = GetStatValue(StatType.ExpGainBonus);
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

    private long GetRequiredExp(int level)
    {
        return level * 100L;
    }

    public bool TryCompareEquipmentPower(
    EquipmentModel currentEquipment,
    EquipmentModel droppedEquipment,
    out double currentPower,
    out double nextPower)
    {
        currentPower = 0d;
        nextPower = 0d;

        if (!IsInitialized ||
            currentEquipment == null ||
            droppedEquipment == null ||
            ReferenceEquals(currentEquipment, droppedEquipment))
        {
            return false;
        }

        var gameManager = GameManager.Instance;
        var owned = gameManager?.SaveServer?.GetEquipments();

        if (owned == null ||
            !owned.Contains(currentEquipment) ||
            !owned.Contains(droppedEquipment) ||
            !currentEquipment.IsEquipped ||
            droppedEquipment.IsEquipped)
        {
            return false;
        }

        var currentData =
            gameManager.Data.GetEquipmentData(currentEquipment.ItemDataId);

        var droppedData =
            gameManager.Data.GetEquipmentData(droppedEquipment.ItemDataId);

        if (currentData == null ||
            droppedData == null ||
            currentData.Type != droppedData.Type)
        {
            return false;
        }

        var currentLoadout = new List<EquipmentModel>();

        foreach (var equipment in owned)
        {
            if (equipment != null && equipment.IsEquipped)
            {
                currentLoadout.Add(equipment);
            }
        }

        var nextLoadout = new List<EquipmentModel>(currentLoadout);
        nextLoadout.Remove(currentEquipment);
        nextLoadout.Add(droppedEquipment);

        return
            TryCalculateLoadoutPower(currentLoadout, out currentPower) &&
            TryCalculateLoadoutPower(nextLoadout, out nextPower);
    }

    private bool TryCalculateLoadoutPower(
        List<EquipmentModel> loadout,
        out double power)
    {
        power = 0d;

        var flatStats = new Dictionary<StatType, float>();

        foreach (var equipment in loadout)
        {
            var data = GameManager.Instance.Data.GetEquipmentData(
                equipment.ItemDataId
            );

            if (data == null)
            {
                return false;
            }

            float value = EquipmentPowerUtility.CalculateEquipmentStat(
                data,
                equipment.Level
            );

            flatStats.TryGetValue(data.MainStatType, out float previous);
            flatStats[data.MainStatType] = previous + value;
        }

        var relicBonuses = _relicManager != null
            ? _relicManager.GetTotalPercentStats()
            : null;

        var stats = _calculator.CalculateAllStats(flatStats, relicBonuses);

        power = EquipmentPowerUtility.CalculatePower(stats);

        return !double.IsNaN(power) && !double.IsInfinity(power);
    }


    public int CurrentLevel => _playerModel != null ? _playerModel.Level : 1;
}