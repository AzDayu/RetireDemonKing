using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
   private List<EquipmentModel> _ownedEquipmentList = new List<EquipmentModel>();

    public void Initialize(List<EquipmentModel> savedEquipmentList)
    {
        _ownedEquipmentList = savedEquipmentList ?? new List<EquipmentModel>();
    }

    public bool HasEquippedEquipment()
    {
        foreach (EquipmentModel equipmentModel in
                 _ownedEquipmentList)
        {
            if (equipmentModel != null &&
                equipmentModel.IsEquipped)
            {
                return true;
            }
        }

        return false;
    }

    public EquipmentModel GetEquippedEquipment(
        EquipmentType equipmentType,
        int typeIndex = 0)
    {
        if (typeIndex < 0 ||
            GameManager.Instance == null ||
            GameManager.Instance.Data == null)
        {
            return null;
        }

        int currentTypeIndex = 0;

        foreach (EquipmentModel equipmentModel in
                 _ownedEquipmentList)
        {
            if (equipmentModel == null ||
                !equipmentModel.IsEquipped)
            {
                continue;
            }

            EquipmentItem equipmentData =
                GameManager.Instance.Data.GetEquipmentData(
                    equipmentModel.ItemDataId
                );

            if (equipmentData == null ||
                equipmentData.Type != equipmentType)
            {
                continue;
            }

            if (currentTypeIndex == typeIndex)
            {
                return equipmentModel;
            }

            currentTypeIndex++;
        }

        return null;
    }

    public bool TryAddEquipment(EquipmentModel equipmentModel)
    {
        if (equipmentModel == null ||
            string.IsNullOrEmpty(equipmentModel.ItemDataId))
        {
            return false;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.Data == null ||
            GameManager.Instance.Data.GetEquipmentData(
                equipmentModel.ItemDataId
            ) == null)
        {
            Debug.LogError(
                $"[EquipmentManager] 존재하지 않는 장비 데이터입니다: " +
                $"{equipmentModel.ItemDataId}"
            );

            return false;
        }

        foreach (EquipmentModel ownedEquipment in
                 _ownedEquipmentList)
        {
            if (ownedEquipment != null &&
                ownedEquipment.ItemUniqueId ==
                equipmentModel.ItemUniqueId)
            {
                Debug.LogWarning(
                    $"[EquipmentManager] 중복된 장비 고유 ID입니다: " +
                    $"{equipmentModel.ItemUniqueId}"
                );

                return false;
            }
        }

        _ownedEquipmentList.Add(equipmentModel);
        return true;
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
