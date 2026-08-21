using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

/*public class InventoryUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private UIButton Button_UseSelectItem;
    [SerializeField] private UIButton Button_CloseSelf;
    [SerializeField] private UIButton Button_CloseSelfAllArea;

    [Header("Dynamic Slots")]
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private InventorySlotUI _slotPrefab;

    private List<InventorySlotUI> _activeSlotViews = new List<InventorySlotUI>();
    private InventoryViewModel _invenVm;

    private void OnEnable()
    {
        Button_UseSelectItem.BindOnClickButtonEvent(OnClick_UseSelectItem, true);
        Button_CloseSelf.BindOnClickButtonEvent(OnClick_ClosePopup);
        Button_CloseSelfAllArea.BindOnClickButtonEvent(OnClick_ClosePopup);

        SetInventoryItemSlotOnEnable();
        ActiveUseSelectItemButton(false);
    }

    private void OnDisable()
    {
        Button_UseSelectItem.UnBindAllOnClickButtonEvent();
        Button_CloseSelf.UnBindAllOnClickButtonEvent();
        Button_CloseSelfAllArea.UnBindAllOnClickButtonEvent();

        UnbindInventoryViewModel();
    }

    private void OnDestroy()
    {
        UnbindInventoryViewModel();
    }

    private void SetInventoryItemSlotOnEnable()
    {
        RemoveAllItemSlot();
        FindInventoryViewModelAndBind();
    }

    private void FindInventoryViewModelAndBind()
    {
        var invenModel = NetworkManager.Instance.InventoryService.GetLocalPlayerInventoryModel();
        if (invenModel == null)
        {
            Debug.LogWarning("인벤토리 모델이 없습니다!");
            return;
        }

        UnbindInventoryViewModel();

        _invenVm = new InventoryViewModel();
        _invenVm.Initialize(invenModel);
        _invenVm.PropertyChanged += OnPropChanged_InvenView;
        _invenVm.OnSelectionChanged += OnSlotSelected;

        ResetItemSlotAndCreateAll();
    }

    private void OnPropChanged_InvenView(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InventoryViewModel.SlotViewModels))
        {
            ResetItemSlotAndCreateAll();
        }
    }

    // 존재하는 슬롯 개수만큼만 UI 프리팹을 생성/매핑
    private void ResetItemSlotAndCreateAll()
    {
        RemoveAllItemSlot();
        ActiveUseSelectItemButton(false);

        if (_invenVm == null || _invenVm.SlotViewModels == null) return;

        foreach (var itemKv in _invenVm.SlotViewModels)
        {
            var slotVm = itemKv.Value;

            var slotView = Instantiate(_slotPrefab, _slotContainer);
            slotView.BindSlotViewModel(slotVm);
            _activeSlotViews.Add(slotView);
        }
    }

    private void OnSlotSelected(InventorySlotViewModel selectedVm)
    {
        if (selectedVm == null || string.IsNullOrEmpty(selectedVm.ItemId))
        {
            ActiveUseSelectItemButton(false);
            return;
        }

        
        ActiveUseSelectItemButton(true);
    }

    private void ActiveUseSelectItemButton(bool isActive)
    {
        if (Button_UseSelectItem != null)
        {
            Button_UseSelectItem.gameObject.SetActive(isActive);
        }
    }

    private void RequestSelectedUseItem()
    {
        var selected = _invenVm?.SelectedSlot;
        if (selected == null) return;

        NetworkManager.Instance.InventoryService.RequestUseItem(selected.GetSlotId());
    }

    public void OnClick_ClosePopup()
    {
        GameManager.Instance.UI.CloseContentUI(UIType.InventoryUI);
    }

    public void OnClick_UseSelectItem()
    {
        RequestSelectedUseItem();
    }

    private void RemoveAllItemSlot()
    {
        foreach (var slotView in _activeSlotViews)
        {
            if (slotView != null)
            {
                slotView.ClearSlot();
                Destroy(slotView.gameObject);
            }
        }
        _activeSlotViews.Clear();
    }

    private void UnbindInventoryViewModel()
    {
        if (_invenVm != null)
        {
            _invenVm.PropertyChanged -= OnPropChanged_InvenView;
            _invenVm.OnSelectionChanged -= OnSlotSelected;
            _invenVm.Dispose();
            _invenVm = null;
        }
    }
}*/