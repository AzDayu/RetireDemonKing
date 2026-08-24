using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public PlayerModel Player = new PlayerModel();
    public List<EquipmentModel> Equipments = new List<EquipmentModel>();
    public List<RelicModel> Relics = new List<RelicModel>();
    public long LastSaveUnixMinutes;
    public string UserAccountId = string.Empty;
}
