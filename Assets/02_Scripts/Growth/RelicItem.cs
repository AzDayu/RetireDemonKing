using System;

[Serializable]
public class RelicItem
{
    public string Id;
    public string Name;
    public StatType TargetStatType;
    public float BasePercentBonus;
    public float PercentBonusPerLevel;
    public EquipmentGrade Grade;
    public int DropWeight;
    public string IconId;
    public string Description;
}