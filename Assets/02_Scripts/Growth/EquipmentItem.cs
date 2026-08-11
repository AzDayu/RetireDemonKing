using System;

[Serializable]
public class EquipmentItem
{
    public string Id;
    public string Name;
    public EquipmentType Type;
    public StatType MainStatType;
    public float BaseStatValue;
    public float StatValuePerLevel;
    public int Level = 1;
    public bool IsEquipped;
}
