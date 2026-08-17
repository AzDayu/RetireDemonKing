using System.Collections.Generic;

public class InventoryNetworkService
{
    private InventoryModel _localPlayerInventoryModel;

    public InventoryModel GetLocalPlayerInventoryModel()
    {
        if (_localPlayerInventoryModel == null)
        {
            CreateLocalPlayerInventoryModel();
        }

        return _localPlayerInventoryModel;
    }

    private InventoryModel CreateLocalPlayerInventoryModel()
    {
        // [수정] 기본 데이터 생성 시 최대 슬롯 기본값 보장 (데이터 클래스 기본값에 따라 조정)
        InventoryData defaultData = new InventoryData();
        if (defaultData.MaxSlotCount <= 0)
        {
            defaultData.MaxSlotCount = 30; // 기본 최대 슬롯 개수
        }

        _localPlayerInventoryModel = new InventoryModel(defaultData);
        return _localPlayerInventoryModel;
    }

    public bool RequestAddItem(string itemDataId, int addItemCount)
    {
        var model = GetLocalPlayerInventoryModel();

        bool isSuccess = model.InputItem(itemDataId, addItemCount);

        if (isSuccess)
        {
            // NetworkManager.Inst.SaveLoadService.RequstSaveData();
        }

        return isSuccess;
    }

    public bool RequestUseItem(long requestUseTargetSlotId)
    {
        var model = GetLocalPlayerInventoryModel();
        var slot = model.GetSlot(requestUseTargetSlotId);

        if (slot == null || string.IsNullOrEmpty(slot.ItemId))
        {
            return false;
        }

        //bool isSuccess = ItemUseHandler.Execute(slot.ItemId);

        //if (isSuccess)
        //{
        //    RequestRemoveItem(requestUseTargetSlotId, 1);
        //    return true;
        //}

        return false;
    }

    private void RequestRemoveItem(long slotId, int count)
    {
        var model = GetLocalPlayerInventoryModel();
        var slot = model.GetSlot(slotId);

        if (slot == null) return;

        int remainCount = slot.Count - count;

        if (remainCount <= 0)
        {
            model.ClearSlot(slotId);
        }
        else
        {
            model.TrySetItem(slotId, slot.ItemId, remainCount);
        }

        // NetworkManager.Inst.SaveLoadService.RequstSaveData();
    }

    public IEnumerable<InventorySlotModel> GetPlayerItemList()
    {
        var model = GetLocalPlayerInventoryModel();
        return model.GetAllSlots();
    }

    public void LoadInventoryData(InventoryData invSaveData)
    {
        if (invSaveData == null) return;

        var model = GetLocalPlayerInventoryModel();

        model.Refresh(invSaveData);
    }
}