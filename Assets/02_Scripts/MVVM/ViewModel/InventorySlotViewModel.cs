using NUnit.Framework.Interfaces;
using System;
using UnityEngine;

public class InventorySlotViewModel : ViewModelBase
{
    private InventorySlotModel _slotModel;
    public event Action<InventorySlotViewModel, bool> OnSelected;

    public InventorySlotViewModel(InventorySlotModel slotmodel)
    {
        _slotModel = slotmodel;
        _slotModel.OnChanged += HandleModelChanged;
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemId));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(IsSelected));
    }

    private void HandleModelChanged()
    {
        OnPropertyChanged(nameof(ItemId));
        OnPropertyChanged(nameof(Count));
    }

    public void Dispose()
    {
        if (_slotModel != null)
        {
            _slotModel.OnChanged -= HandleModelChanged;
            _slotModel = null;
        }
        OnSelected = null;
    }

    public string ItemId
    {
        get => _slotModel != null ? _slotModel.ItemId : string.Empty;
        set
        {
            if (_slotModel != null && _slotModel.ItemId != value)
            {
                _slotModel.ItemId = value;
                OnPropertyChanged(nameof(ItemId));
            }
        }
    }

    public int Count
    {
        get => _slotModel != null ? _slotModel.Count : 0;
        set
        {
            if (_slotModel != null && _slotModel.Count != value && value >= 0)
            {
                _slotModel.Count = value;
                OnPropertyChanged(nameof(Count));
            }
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public void ButtonClicked()
    {
        OnSelected?.Invoke(this, IsSelected);
    }

    public long GetSlotId()
    {
        return _slotModel != null ? _slotModel.SlotId : -1;
    }

}
