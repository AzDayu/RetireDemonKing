using System;

public class InventorySlotModel
{
    public event Action OnChanged;

    public long SlotId { get; }

    private string _itemId;
    public string ItemId
    {
        get => _itemId;
        set
        {
            if (_itemId == value) return;
            _itemId = value;
            OnChanged?.Invoke();
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            if (_count == value) return;
            _count = value;
            OnChanged?.Invoke();
        }
    }

    public InventorySlotModel(long slotId, string itemId, int count)
    {
        SlotId = slotId;
        _itemId = itemId;
        _count = count;
    }
}
