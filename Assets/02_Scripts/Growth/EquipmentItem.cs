using System;

[Serializable]
public class EquipmentItem
{
    public string Id;
    public string Name;
    public EquipmentType Type;
    public EquipmentGrade Grade;
    public StatType MainStatType;
    public float BaseStatValue;
    public float StatValuePerLevel;
    public float GradeMultiplier = 1f;
    public int DropWeight;
    public string PrefabId;
    public string IconId;
    public string Description;
}
