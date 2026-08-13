using System;
using System.Collections.Generic;
using UnityEngine;

public class StatCalculator
{
    private readonly Dictionary<StatType, float> _baseStats = new Dictionary<StatType, float>();

    public void SetBaseStat(StatType statType, float value)
    {
        _baseStats[statType] = value;
    }

    public float GetBaseStat(StatType statType)
    {
        return _baseStats.TryGetValue(statType, out float val) ? val : 0f;
    }

    public float CalculateFinalStat(StatType statType, float flatBonus, float percentBonus)
    {
        float baseVal = GetBaseStat(statType);

        float totalFlat = baseVal + flatBonus;
        float multiplier = 1f + (percentBonus / 100f);

        float finalStat = totalFlat * multiplier;
        return Mathf.Max(0f, finalStat);
    }

    public Dictionary<StatType, float> CalculateAllStats(
        Dictionary<StatType, float> flatBonuses,
        Dictionary<StatType, float> percentBonuses)
    {
        var finalStatsMap = new Dictionary<StatType, float>();

        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float flat = (flatBonuses != null && flatBonuses.TryGetValue(statType, out float fVal)) ? fVal : 0f;
            float percent = (percentBonuses != null && percentBonuses.TryGetValue(statType, out float pVal)) ? pVal : 0f;

            finalStatsMap[statType] = CalculateFinalStat(statType, flat, percent);
        }

        return finalStatsMap;
    }
}