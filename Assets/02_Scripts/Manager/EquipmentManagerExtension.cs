using System.Collections.Generic;
using UnityEngine;

public static class EquipmentManagerExtension
{
    private const int MaxEquippedRingCount = 2;

    public static List<EquipmentModel> GetEquippedItems(
        this EquipmentManager equipmentManager,
        EquipmentType equipmentType)
    {
        var result = new List<EquipmentModel>();
        List<EquipmentModel> equipmentList = GameManager.Instance?.SaveServer?.GetEquipments();

        if (equipmentManager == null || equipmentList == null || GameManager.Instance?.Data == null)
        {
            return result;
        }

        foreach (EquipmentModel model in equipmentList)
        {
            if (model == null || !model.IsEquipped)
            {
                continue;
            }

            EquipmentItem data =
                GameManager.Instance.Data.GetEquipmentData(model.ItemDataId);
            if (data != null && data.Type == equipmentType)
            {
                result.Add(model);
            }
        }

        return result;
    }

    public static bool TryEquipDroppedItem(
        this EquipmentManager equipmentManager,
        EquipmentModel targetEquipment)
    {
        if (!TryGetOwnedEquipmentData(
                equipmentManager,
                targetEquipment,
                out EquipmentItem targetData))
        {
            return false;
        }

        if (targetData.Type != EquipmentType.Ring)
        {
            equipmentManager.EquipItem(targetEquipment);
            return targetEquipment.IsEquipped;
        }

        List<EquipmentModel> equippedRings =
            equipmentManager.GetEquippedItems(EquipmentType.Ring);
        if (!targetEquipment.IsEquipped &&
            equippedRings.Count >= MaxEquippedRingCount)
        {
            return false;
        }

        targetEquipment.IsEquipped = true;
        GameManager.Instance.Growth.RecalculateTotalStats();
        return true;
    }

    public static bool TryApplyRingSelection(
        this EquipmentManager equipmentManager,
        IReadOnlyCollection<EquipmentModel> selectedRings)
    {
        if (equipmentManager == null || selectedRings == null ||
            selectedRings.Count != MaxEquippedRingCount)
        {
            return false;
        }

        List<EquipmentModel> equipmentList =
            GameManager.Instance?.SaveServer?.GetEquipments();
        if (equipmentList == null || GameManager.Instance?.Data == null)
        {
            return false;
        }

        var selectedSet = new HashSet<EquipmentModel>(selectedRings);
        foreach (EquipmentModel selectedRing in selectedSet)
        {
            if (!TryGetOwnedEquipmentData(
                    equipmentManager,
                    selectedRing,
                    out EquipmentItem selectedData) ||
                selectedData.Type != EquipmentType.Ring)
            {
                return false;
            }
        }

        foreach (EquipmentModel model in equipmentList)
        {
            if (model == null)
            {
                continue;
            }

            EquipmentItem data =
                GameManager.Instance.Data.GetEquipmentData(model.ItemDataId);
            if (data != null && data.Type == EquipmentType.Ring)
            {
                model.IsEquipped = selectedSet.Contains(model);
            }
        }

        GameManager.Instance.Growth.RecalculateTotalStats();
        return true;
    }

    private static bool TryGetOwnedEquipmentData(
        EquipmentManager equipmentManager,
        EquipmentModel equipmentModel,
        out EquipmentItem equipmentData)
    {
        equipmentData = null;
        List<EquipmentModel> equipmentList =
            GameManager.Instance?.SaveServer?.GetEquipments();

        if (equipmentManager == null || equipmentModel == null ||
            equipmentList == null || !equipmentList.Contains(equipmentModel) ||
            GameManager.Instance?.Data == null)
        {
            return false;
        }

        equipmentData =
            GameManager.Instance.Data.GetEquipmentData(equipmentModel.ItemDataId);
        return equipmentData != null;
    }
}
