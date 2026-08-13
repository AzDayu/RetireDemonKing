using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
   private List<EquipmentModel> _ownedEquipmentList = new List<EquipmentModel>();

    public void Initialize(List<EquipmentModel> savedEquipmentList)
    {
        _ownedEquipmentList = savedEquipmentList ?? new List<EquipmentModel>();
    }

    public Dictionary<StatType, float> GetTotalFlatStats()
    {
        var flatStatsMap = new Dictionary<StatType, float>();

        foreach (var itemModel in _ownedEquipmentList)
        {
            if (itemModel.IsEquipped == false) continue;

            EquipmentItem staticData = GameManager.Instance.Data.GetEquipmentData(itemModel.ItemDataId);
            if (staticData == null) continue;

            float levelBonus = (itemModel.Level - 1) * staticData.StatValuePerLevel;
            float finalStatValue = (staticData.BaseStatValue + levelBonus) * staticData.GradeMultiplier;

            if (flatStatsMap.ContainsKey(staticData.MainStatType))
            {
                flatStatsMap[staticData.MainStatType] += finalStatValue;
            }
            else
            {
                flatStatsMap[staticData.MainStatType] = finalStatValue;
            }
        }

        return flatStatsMap;
    }

    public void EquipItem(EquipmentModel targetItemModel)
    {
        if (targetItemModel == null) return;

        EquipmentItem targetStaticData = GameManager.Instance.Data.GetEquipmentData(targetItemModel.ItemDataId);
        if (targetStaticData == null) return;

        foreach (var ownedItem in _ownedEquipmentList)
        {
            EquipmentItem ownedStaticData = GameManager.Instance.Data.GetEquipmentData(ownedItem.ItemDataId);
            if (ownedStaticData != null && ownedStaticData.Type == targetStaticData.Type && ownedItem.IsEquipped)
            {
                ownedItem.IsEquipped = false;
            }
        }

        targetItemModel.IsEquipped = true;

        GameManager.Instance.Growth.RecalculateTotalStats();
    }

    public bool TryEnhanceEquipment(EquipmentModel targetItemModel, PlayerModel playerModel)
    {

        EquipmentItem staticData = GameManager.Instance.Data.GetEquipmentData(targetItemModel.ItemDataId);
        if (staticData == null) return false;

        int gradeWeight = ((int)staticData.Grade + 1);
        long cost = targetItemModel.Level * 50L * gradeWeight;

        if (playerModel.EnhanceCurrency < cost) return false;

        playerModel.EnhanceCurrency -= cost;
        targetItemModel.Level++;

        GameManager.Instance.Growth.RecalculateTotalStats();
        return true;
    }

    public void DismantleItem(EquipmentModel targetItemModel, PlayerModel playerModel)
    {
        if (targetItemModel == null || targetItemModel.IsEquipped) return;

        EquipmentItem staticData = GameManager.Instance.Data.GetEquipmentData(targetItemModel.ItemDataId);
        if (staticData == null) return;

        int gradeWeight = ((int)staticData.Grade + 1);
        long rewardAmount = targetItemModel.Level * 20L * gradeWeight;

        playerModel.EnhanceCurrency += rewardAmount;
        _ownedEquipmentList.Remove(targetItemModel);

        Debug.Log($"[EquipmentManager] 장비 분해 완료! 획득한 육성 재화: {rewardAmount}");
    }
}
