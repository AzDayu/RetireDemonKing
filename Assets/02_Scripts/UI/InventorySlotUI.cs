using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : UIBase
{
    [SerializeField] private Text Text_StackCount;
    [SerializeField] private UIButton Button_Slot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Frame;
    [SerializeField] private Image Image_Selected;

    private InventorySlotViewModel _vm;

    public long SlotId { get; private set; }
    public bool IsUsableItem { get; private set; }

    private void OnEnable()
    {
        if (Image_Selected != null)
        {
            Image_Selected.gameObject.SetActive(false);
        }

        if (Button_Slot != null)
        {
            Button_Slot.BindOnClickButtonEvent(OnClick_SelectItem);
        }
    }

    private void OnDisable()
    {
        if (Button_Slot != null)
        {
            Button_Slot.UnBindAllOnClickButtonEvent();
        }
        UnbindViewModel();
    }

    public void BindSlotViewModel(InventorySlotViewModel slotVm)
    {
        UnbindViewModel();

        if (slotVm == null)
        {
            ClearSlot();
            return;
        }

        gameObject.SetActive(true);
        _vm = slotVm;
        SlotId = _vm.GetSlotId();

        // [수정] ViewModelBase 이벤트 또는 OnInfoChanged/PropertyChanged 연동
        //_vm.PropertyChanged += OnPropChanged_View;
        _vm.InvokeOnceOnInit();
    }

    private void UnbindViewModel()
    {
        if (_vm != null)
        {
            // [수정] 이벤트 구독 해제
            //_vm.PropertyChanged -= OnPropChanged_View;
            _vm = null;
        }
    }

    // [수정] ViewModelBase 규격(string 프로퍼티 이름 전달 방식)에 맞춘 UI 갱신
    private void OnPropChanged_View(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(InventorySlotViewModel.ItemId):
                UpdateItemIcon();
                break;

            case nameof(InventorySlotViewModel.Count):
                if (Text_StackCount != null && _vm != null)
                {
                    Text_StackCount.text = _vm.Count > 1 ? $"{_vm.Count}" : string.Empty;
                }
                break;

            case nameof(InventorySlotViewModel.IsSelected):
                if (Image_Selected != null && _vm != null)
                {
                    Image_Selected.gameObject.SetActive(_vm.IsSelected);
                }
                break;
        }
    }

    public async UniTaskVoid UpdateItemIcon()
    {
        if (_vm == null || Image_Icon == null) return;

        string currentItemId = _vm.ItemId;

        if (string.IsNullOrEmpty(currentItemId))
        {
            Image_Icon.sprite = null;
            Image_Icon.enabled = false;
            return;
        }

        // 어드레서블 주소 규칙: ItemId (예: "Item_1001" 등)
        Sprite iconSprite = await ResourceManager.Inst.LoadSprite(currentItemId);

        if (_vm == null || _vm.ItemId != currentItemId || Image_Icon == null)
        {
            return;
        }

        if (iconSprite != null)
        {
            Image_Icon.sprite = iconSprite;
            Image_Icon.enabled = true;
        }
        else
        {
            Image_Icon.sprite = null;
            Image_Icon.enabled = false;
        }
    }

    public void OnClick_SelectItem()
    {
        _vm?.ButtonClicked();
        Debug.Log($"[{SlotId}] 슬롯 클릭됨");
    }

    public void ClearSlot()
    {
        if (_vm != null)
        {
            _vm.IsSelected = false;
        }
        UnbindViewModel();

        SlotId = -1;
        IsUsableItem = false;

        if (Image_Icon != null)
        {
            Image_Icon.sprite = null;
            Image_Icon.enabled = false;
        }
        if (Text_StackCount != null)
        {
            Text_StackCount.text = string.Empty;
        }
        if (Image_Selected != null)
        {
            Image_Selected.gameObject.SetActive(false);
        }
    }
}
