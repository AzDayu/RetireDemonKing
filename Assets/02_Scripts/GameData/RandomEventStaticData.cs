using System;

[Serializable]
public class RandomEventStaticData
{
    public string Id;
    public string Title;
    public string Description;

    public string Choice1Text;
    public float GoldStageMultiplier;

    public string Choice2Text;
    public StatType BuffStatType;
    public float BuffPercent;
    public float BuffDurationSec;

    public int Weight;
}
