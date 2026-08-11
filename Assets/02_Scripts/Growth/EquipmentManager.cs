using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("=== 보유 장비 리스트 ===")]
    [SerializeField] private List<EquipmentItem> _ownedEquipmentList = new List<EquipmentItem>();

    public void Initialize()
    {
        // 초기 장비 세팅 또는 로드 데이터 연결
    }

    public Dictionary<StatType, float> GetTotalFlatStats()
    {
        var flatStatsMap = new Dictionary<StatType, float>();

        foreach (var item in _ownedEquipmentList)
        {
            if (item.IsEquipped)
            {
                float currentValue = item.BaseStatValue + ((item.Level - 1) * item.StatValuePerLevel);

                if (flatStatsMap.ContainsKey(item.MainStatType))
                {
                    flatStatsMap[item.MainStatType] += currentValue;
                }
                else
                {
                    flatStatsMap[item.MainStatType] = currentValue;
                }
            }
        }

        return flatStatsMap;
    }

    public void EquipItem(EquipmentItem targetItem)
    {
        if (targetItem == null) return;

        foreach (var item in _ownedEquipmentList)
        {
            if (item.Type == targetItem.Type && item.IsEquipped)
            {
                item.IsEquipped = false;
            }
        }

        targetItem.IsEquipped = true;

        GameManager.Instance.Growth.RecalculateTotalStats();
    }

    public void DismantleItem(EquipmentItem targetItem, PlayerModel playerModel)
    {
        if (targetItem == null || targetItem.IsEquipped) return;

        long rewardCurrency = targetItem.Level * 20L;
        playerModel.EnhanceCurrency += rewardCurrency;

        _ownedEquipmentList.Remove(targetItem);

        Debug.Log($"[EquipmentManager] 장비 분해 완료! 획득 육성 재화: {rewardCurrency}");
    }

    public bool TryEnhanceEquipment(EquipmentItem targetItem, PlayerModel playerModel)
    {
        long cost = targetItem.Level * 50L;

        if (playerModel.EnhanceCurrency < cost) return false;

        playerModel.EnhanceCurrency -= cost;
        targetItem.Level++;

        GameManager.Instance.Growth.RecalculateTotalStats();
        return true;
    }
}
