using System;

[Serializable]
public class ActiveBuff
{
    public StatType TargetStat;
    public float PercentValue;
    public float RemainingSeconds;

    public ActiveBuff(StatType targetStat, float percentValue, float durationSec)
    {
        TargetStat = targetStat;
        PercentValue = percentValue;
        RemainingSeconds = durationSec;
    }
}
