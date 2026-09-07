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
    public string PrefabName;
    public float MaxHp;
    public int DropCoins;
    public float DropExp;
    public float AttackPower;
    public float AttackSpeed;
    public List<DropTableData> DropTable;
}

[Serializable]
public class DropTableData
{
    public string MonsterId;
    public float CommonDropRate;
    public float RareDropRate;
    public float EpicDropRate;
    public float LegendaryDropRate;
    public float MythDropRate;
}

