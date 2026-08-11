using System;

[Serializable]
public class RelicItem
{
    public string Id;
    public string Name;
    public StatType TargetStatType;
    public float BasePercentBonus;
    public float PercentBonusPerLevel;
    public int Level = 0;
    public string Grade;
    public int DropWeight;
    public string Description;
}