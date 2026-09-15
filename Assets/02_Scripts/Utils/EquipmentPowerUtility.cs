using System;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentPowerUtility
{
    private const double HpWeight = 0.2d;
    private const double DefenseWeight = 2d;
    private const double EvasionWeight = 3d;
    private const double HealingWeight = 2d;

    public static float CalculateEquipmentStat(EquipmentItem data, int level)
    {
        return (data.BaseStatValue + (Mathf.Max(1, level) - 1) * data.StatValuePerLevel) * data.GradeMultiplier;
    }

    public static double CalculatePower(Dictionary<StatType, float> stats)
    {
        double attack = Get(stats, StatType.Attack);

        double attackSpeed = Get(stats, StatType.AttackSpeed);

        double criticalChance = Math.Min(100d, Get(stats, StatType.CriticalChance)) / 100d;

        double criticalMultiplier = Get(stats, StatType.CriticalDamage) / 100d;

        double averageDamageMultiplier = (1d - criticalChance) + criticalChance * criticalMultiplier;

        double offense = attack * attackSpeed * averageDamageMultiplier;

        double healing = offense * Get(stats, StatType.LifeSteal) / 100d;

        return offense + Get(stats, StatType.MaxHp) * HpWeight + Get(stats, StatType.Defense) * DefenseWeight +
            Math.Min(90d, Get(stats, StatType.Evasion)) * EvasionWeight + healing * HealingWeight;
    }

    public static bool IsStrictlyLower(double next, double current)
    {
        double tolerance = Math.Max(1d, Math.Max(Math.Abs(next), Math.Abs(current))) * 0.000001d;

        return next < current - tolerance;
    }

    private static double Get(Dictionary<StatType, float> stats, StatType type)
    {
        return stats.TryGetValue(type, out float value) ? Math.Max(0d, value) : 0d;
    }
}
