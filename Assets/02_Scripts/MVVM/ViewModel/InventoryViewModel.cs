using System;
using System.Collections.Generic;

public class InventoryViewModel : ViewModelBase
{
    private InventoryModel _inventoryModel;
    private Dictionary<long, InventorySlotViewModel> _slotViewModels = new Dictionary<long, InventorySlotViewModel>();
    public IReadOnlyDictionary<long, InventorySlotViewModel> SlotViewModels => _slotViewModels;

    private InventorySlotViewModel _selectedSlot;
    public InventorySlotViewModel SelectedSlot => _selectedSlot;
    public event Action<InventorySlotViewModel> OnSelectionChanged;

    public void Initialize(InventoryModel inventoryModel)
    {
        UnbindModelEvents();
        ClearAndDisposeSlots();

        _inventoryModel = inventoryModel;

        if (_inventoryModel != null)
        {
            _inventoryModel.OnSlotAdded += HandleSlotAdded;
            _inventoryModel.OnSlotRemoved += HandleSlotRemoved;

            foreach (var slotModel in _inventoryModel.GetAllSlots())
            {
                CreateAndAddSlotViewModel(slotModel);
            }
        }

        OnPropertyChanged(nameof(SlotViewModels));
    }

    private void CreateAndAddSlotViewModel(InventorySlotModel slotModel)
    {
        var slotVm = new InventorySlotViewModel(slotModel);
        slotVm.OnSelected += OnSlotSelected;
        slotVm.InvokeOnceOnInit();

        _slotViewModels.Add(slotModel.SlotId, slotVm);
    }

    private void HandleSlotAdded(InventorySlotModel slotModel)
    {
        CreateAndAddSlotViewModel(slotModel);
        OnPropertyChanged(nameof(SlotViewModels));
    }

    private void HandleSlotRemoved(long slotId)
    {
        if (_slotViewModels.TryGetValue(slotId, out var slotVm))
        {
            if (_selectedSlot == slotVm)
            {
                _selectedSlot = null;
                OnSelectionChanged?.Invoke(null);
            }

            slotVm.OnSelected -= OnSlotSelected;
            slotVm.Dispose();
            _slotViewModels.Remove(slotId);

            OnPropertyChanged(nameof(SlotViewModels));
        }
    }

    private void OnSlotSelected(InventorySlotViewModel clickedVm, bool isSelected)
    {
        if (_selectedSlot != null && _selectedSlot != clickedVm)
        {
            _selectedSlot.IsSelected = false;
        }

        clickedVm.IsSelected = true;
        _selectedSlot = clickedVm;
        OnSelectionChanged?.Invoke(clickedVm);
    }

    public InventorySlotViewModel GetSlotViewModel(long slotId)
    {
        return _slotViewModels.TryGetValue(slotId, out var slotVm) ? slotVm : null;
    }

    private void UnbindModelEvents()
    {
        if (_inventoryModel != null)
        {
            _inventoryModel.OnSlotAdded -= HandleSlotAdded;
            _inventoryModel.OnSlotRemoved -= HandleSlotRemoved;
        }
    }

    private void ClearAndDisposeSlots()
    {
        foreach (var slotVm in _slotViewModels.Values)
        {
            if (slotVm != null)
            {
                slotVm.OnSelected -= OnSlotSelected;
                slotVm.Dispose();
            }
        }
        _slotViewModels.Clear();
        _selectedSlot = null;
    }

    public void Dispose()
    {
        UnbindModelEvents();
        ClearAndDisposeSlots();
    }
}