using System;
using System.Collections.Generic;

public class InventoryModel
{
    private const string EMPTY_ITEM_ID = "";

    // 런타임에 동적으로 관리할 슬롯 딕셔너리
    private Dictionary<long, InventorySlotModel> _slots = new Dictionary<long, InventorySlotModel>();

    private InventoryData _inventoryData;
    private long _nextSlotId = 0;

    public event Action<InventorySlotModel> OnSlotAdded;
    public event Action<long> OnSlotRemoved;

    public InventoryModel(InventoryData inventoryData)
    {
        if (inventoryData == null) return;
        Init(inventoryData);
    }

    public void Refresh(InventoryData inventoryData)
    {
        if (inventoryData == null) return;

        _slots.Clear();
        _nextSlotId = 0;
        Init(inventoryData);
    }

    private void Init(InventoryData inventoryData)
    {
        _inventoryData = inventoryData;

        // 아이템이 존재하는 데이터만 슬롯으로 생성
        foreach (var item in inventoryData.InventoryItems)
        {
            if (!string.IsNullOrEmpty(item.ItemId) && item.Count > 0)
            {
                long slotId = item.SlotIndex >= 0 ? item.SlotIndex : _nextSlotId++;
                var slot = new InventorySlotModel(slotId, item.ItemId, item.Count);
                _slots.Add(slotId, slot);

                if (slotId >= _nextSlotId)
                {
                    _nextSlotId = slotId + 1;
                }
            }
        }
    }

    public bool TrySetItem(long slotId, string itemId, int count)
    {
        if (!_slots.TryGetValue(slotId, out var slot)) return false;

        slot.ItemId = itemId;
        slot.Count = count;

        if (count <= 0 || string.IsNullOrEmpty(itemId))
        {
            RemoveSlot(slotId);
        }

        return true;
    }

    public bool InputItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return false;

        int remainingCount = count;
        int maxCount = _inventoryData.MaxItemCount;

        foreach (var slot in _slots.Values)
        {
            if (slot.ItemId == itemId && slot.Count < maxCount)
            {
                int spaceLeft = maxCount - slot.Count;
                int addCount = Math.Min(remainingCount, spaceLeft);

                slot.Count += addCount;
                remainingCount -= addCount;

                if (remainingCount <= 0)
                {
                    return true;
                }
            }
        }

        // 오버 카운트 신규 슬롯을 생성하여 적재
        while (remainingCount > 0)
        {
            if (_slots.Count >= _inventoryData.MaxSlotCount)
            {
                return false;
            }

            int addCount = Math.Min(remainingCount, maxCount);
            long newSlotId = _nextSlotId++;
            var newSlot = new InventorySlotModel(newSlotId, itemId, addCount);

            _slots.Add(newSlotId, newSlot);
            OnSlotAdded?.Invoke(newSlot);

            remainingCount -= addCount;
        }

        return true;
    }

    public bool RemoveSlot(long slotId)
    {
        if (_slots.Remove(slotId))
        {
            OnSlotRemoved?.Invoke(slotId);
            return true;
        }
        return false;
    }

    public bool ClearSlot(long slotId)
    {
        return RemoveSlot(slotId);
    }

    public InventorySlotModel GetSlot(long slotId)
    {
        return _slots.TryGetValue(slotId, out var slot) ? slot : null;
    }

    public IEnumerable<InventorySlotModel> GetAllSlots() => _slots.Values;

    public int GetCurrentSlotCount() => _slots.Count;
    public int GetMaxSlotCount() => _inventoryData != null ? _inventoryData.MaxSlotCount : 0;
    public int GetMaxItemCount() => _inventoryData != null ? _inventoryData.MaxItemCount : 0;

    public InventoryData CaptureInventoryData()
    {
        _inventoryData.InventoryItems.Clear();

        foreach (var item in _slots)
        {
            _inventoryData.InventoryItems.Add(new InventoryItemData(item.Key, item.Value.ItemId, item.Value.Count));
        }

        return _inventoryData;
    }
}