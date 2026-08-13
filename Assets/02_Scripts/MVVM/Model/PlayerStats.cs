using System;
using System.Collections.Generic;

public enum StatType
{
    BaseAttackPower,
    BaseMaxHP,
    Defense,
    AttackSpeed,
    CritChance
}

public enum ModifierType
{
    Flat,
    Percent
}

[System.Serializable]
public class StatModifier
{
    public StatType Type;
    public ModifierType ModType;
    public float Value;
}

public class PlayerStats
{
    // 기본 스탯
    private readonly Dictionary<StatType, float> _baseStats = new Dictionary<StatType, float>();

    // 증가값 내용 (Key: sourceId - 아이템ID, 스킬ID 등)
    private readonly Dictionary<string, List<StatModifier>> _modifiers = new Dictionary<string, List<StatModifier>>();

    // 연산 합
    private readonly Dictionary<StatType, float> _flatCache = new Dictionary<StatType, float>();
    private readonly Dictionary<StatType, float> _percentCache = new Dictionary<StatType, float>();

    public event Action<StatType> OnStatUpdated;

    public PlayerStats()
    {
        // Enum 초기화
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            _baseStats[type] = 0f;
        }
    }

    // 기본 스탯 초기화
    //public void InitializeBaseStats(PlayerStatData playerStatData)
    //{
    //    if (playerStatData == null) return;

    //    _baseStats[StatType.AttackPower] = playerStatData.Atk;
    //    _baseStats[StatType.MaxHP] = playerStatData.HP;
    //    _baseStats[StatType.MaxMP] = playerStatData.MP;
    //    _baseStats[StatType.AttackSpeed] = playerStatData.AtkSpeed;

    //    NotifyAllStatsUpdated();
    //}

    // 기본 스탯 개별 직접 설정
    public void SetBaseStat(StatType type, float value)
    {
        _baseStats[type] = value;
        OnStatUpdated?.Invoke(type);
    }

    public void SetModifier(string sourceId, List<StatModifier> modifierList)
    {
        if (string.IsNullOrEmpty(sourceId) || modifierList == null) return;

        _modifiers[sourceId] = modifierList;
        RecalculateStats();
    }

    public void RemoveModifier(string sourceId)
    {
        if (_modifiers.Remove(sourceId))
        {
            RecalculateStats();
        }
    }

    // 최종 스탯 반환
    public float GetValue(StatType type)
    {
        _baseStats.TryGetValue(type, out float baseValue);
        _flatCache.TryGetValue(type, out float flatValue);
        _percentCache.TryGetValue(type, out float percentValue);

        return (baseValue + flatValue) * (1f + percentValue);
    }

    private void RecalculateStats()
    {
        _flatCache.Clear();
        _percentCache.Clear();

        foreach (var modifierList in _modifiers.Values)
        {
            foreach (var mod in modifierList)
            {
                if (mod.ModType == ModifierType.Flat)
                {
                    _flatCache.TryGetValue(mod.Type, out float currentFlat);
                    _flatCache[mod.Type] = currentFlat + mod.Value;
                }
                else if (mod.ModType == ModifierType.Percent)
                {
                    _percentCache.TryGetValue(mod.Type, out float currentPercent);
                    _percentCache[mod.Type] = currentPercent + mod.Value;
                }
            }
        }

        // 전체 스탯 업데이트 알림
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            OnStatUpdated?.Invoke(type);
        }
    }

    public void ClearAllData()
    {
        _modifiers.Clear();
        _flatCache.Clear();
        _percentCache.Clear();
        NotifyAllStatsUpdated();
    }

    private void NotifyAllStatsUpdated()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            OnStatUpdated?.Invoke(statType);
        }
    }
}
