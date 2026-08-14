using System;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{

}

[Serializable]
public class MonsterData
{
    public string MonsterId;
    public string MonsterName;
    public float MaxHp;
    public float AttackPower;
    public float AttackSpeed;
    public int DropCoins;
    public float DropExp;
    public List<DropItemData> DropTable;
}

[Serializable]
public class DropItemData
{
    public string ItemId;
    public float DropRate;
    public int MinCount;
    public int MaxCount;

    public DropItemData(string itemId, float dropRate, int minCount = 1, int maxCount = 1)
    {
        ItemId = itemId;
        DropRate = dropRate;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}