using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("=== 드랍 확률 설정 ===")]
    [SerializeField] private float _baseMonsterItemDropRate = 0.05f;

    private readonly Dictionary<EquipmentGrade, float> _gradeWeights = new Dictionary<EquipmentGrade, float>
    {
        { EquipmentGrade.Common, 70f },
        { EquipmentGrade.Rare, 20f },
        { EquipmentGrade.Epic, 7.5f },
        { EquipmentGrade.Legendary, 2f },
        { EquipmentGrade.Mythic, 0.5f }
    };

    public void ProcessMonsterReward(int playerLevel)
    {
        float dropBonusPercent = GameManager.Instance.Growth != null
            ? GameManager.Instance.Growth.GetStatValue(StatType.EquipmentDropRate)
            : 0f;

        float finalDropRate = _baseMonsterItemDropRate * (1f + (dropBonusPercent / 100f));

        if (UnityEngine.Random.value > finalDropRate) return;

        EquipmentModel droppedEquipment = GenerateEquipmentWithPlayerLevel(playerLevel);

        if (droppedEquipment != null)
        {
            GameManager.Instance.Growth.Equipment.TryAddEquipment(droppedEquipment);
            Debug.Log($"[DropManager] 장비 드랍 성공! ID: {droppedEquipment.ItemDataId} | Lv.{droppedEquipment.Level}");
        }
    }

    public EquipmentModel GenerateEquipmentWithPlayerLevel(int playerLevel)
    {
        List<EquipmentItem> allEquipment = GameManager.Instance.Data.GetAllEquipmentDataList();
        if (allEquipment == null || allEquipment.Count == 0) return null;

        EquipmentGrade targetGrade = RollGrade();

        List<EquipmentItem> matchingItems = allEquipment
            .Where(item => item.Grade == targetGrade)
            .ToList();

        if (matchingItems.Count == 0)
        {
            matchingItems = allEquipment;
        }

        int randomIndex = UnityEngine.Random.Range(0, matchingItems.Count);
        EquipmentItem selectedStaticData = matchingItems[randomIndex];

        int minLevel = Mathf.Max(1, playerLevel - 2);
        int maxLevel = playerLevel + 1;
        int calculatedLevel = UnityEngine.Random.Range(minLevel, maxLevel + 1);

        return new EquipmentModel
        {
            ItemUniqueId = DateTime.UtcNow.Ticks,
            ItemDataId = selectedStaticData.Id,
            Level = calculatedLevel,
            IsEquipped = false
        };
    }

    private EquipmentGrade RollGrade()
    {
        float totalWeight = _gradeWeights.Values.Sum();
        float randomVal = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var pair in _gradeWeights)
        {
            cumulative += pair.Value;
            if (randomVal <= cumulative)
            {
                return pair.Key;
            }
        }

        return EquipmentGrade.Common;
    }

    public void GrantMonsterCurrencyAndExp(MonsterData monsterData)
    {
        if (monsterData == null)
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;
        PlayerModel player = gameManager?.SaveServer?.GetPlayerModel();
        GrowthManager growth = gameManager?.Growth;

        if (player == null || growth == null || !growth.IsInitialized)
        {
            Debug.LogError(
                "[DropManager] 플레이어 초기화가 완료되지 않아 보상을 지급하지 못했습니다."
            );
            return;
        }

        double goldMultiplier = Math.Max(
            0d,
            1d + growth.GetStatValue(StatType.GoldGainBonus) / 100d
        );

        long goldReward = (long)Math.Floor(
            Math.Max(0, monsterData.DropCoins) * goldMultiplier
        );

        long baseExp = (long)Math.Floor(
            Math.Max(0d, monsterData.DropExp)
        );

        player.Gold += goldReward;

        growth.AddExp(baseExp);

        Debug.Log(
            $"[DropManager] 처치 보상: " +
            $"골드 +{goldReward}, 기본 경험치 +{baseExp}"
        );
    }
}