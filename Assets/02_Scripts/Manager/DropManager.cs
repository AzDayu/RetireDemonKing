using System;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float _baseMonsterItemDropRate = 0.05f;

    private static long _idCounter = 0;

    public void ProcessMonsterReward(MonsterData monsterData)
    {
        if (monsterData == null) return;

        var saveServer = GameManager.Instance.SaveServer;
        var player = saveServer?.GetPlayerModel();
        if (player == null) return;

        if (monsterData.DropExp > 0 && GameManager.Instance.Growth != null)
        {
            GameManager.Instance.Growth.AddExp((long)monsterData.DropExp);
        }

        float goldBonusPercent = GameManager.Instance.Growth.GetStatValue(StatType.GoldGainBonus);
        long finalGold = Mathf.RoundToInt(monsterData.DropCoins * (1f + (goldBonusPercent / 100f)));
        player.Gold += finalGold;

        float dropBonusPercent = GameManager.Instance.Growth.GetStatValue(StatType.EquipmentDropRate);
        float finalDropRate = _baseMonsterItemDropRate + (dropBonusPercent / 100f);

        if (UnityEngine.Random.value <= finalDropRate)
        {
            GenerateAndRewardEquipment(player, monsterData);
        }
    }

    private void GenerateAndRewardEquipment(PlayerModel player, MonsterData monsterData)
    {
        string targetItemDataId = GetRandomItemIdFromDropTable(monsterData.DropTable);

        if (string.IsNullOrEmpty(targetItemDataId))
        {
            targetItemDataId = "eq_weapon_Common";
        }

        int minLevel = Mathf.Max(1, player.Level - 2);
        int maxLevel = player.Level + 1;
        int droppedEquipLevel = UnityEngine.Random.Range(minLevel, maxLevel + 1);

        long uniqueId = DateTime.UtcNow.Ticks + (_idCounter++);

        EquipmentModel newEquipment = new EquipmentModel()
        {
            ItemUniqueId = uniqueId,
            ItemDataId = targetItemDataId,
            Level = droppedEquipLevel,
            IsEquipped = false
        };

        if (GameManager.Instance.Growth?.Equipment != null)
        {
            bool isSuccess = GameManager.Instance.Growth.Equipment.TryAddEquipment(newEquipment);
            if (isSuccess)
            {
                Debug.Log($"[DropManager]  장비 드랍 성공! ID: {targetItemDataId} (Lv.{droppedEquipLevel})");
            }
        }
    }

    private string GetRandomItemIdFromDropTable(List<DropItemData> dropTable)
    {
        if (dropTable == null || dropTable.Count == 0) return null;

        float randomVal = UnityEngine.Random.value;
        float cumulativeRate = 0f;

        foreach (var dropItem in dropTable)
        {
            cumulativeRate += dropItem.DropRate;
            if (randomVal <= cumulativeRate)
            {
                return dropItem.ItemId;
            }
        }

        return dropTable[0].ItemId;
    }
}