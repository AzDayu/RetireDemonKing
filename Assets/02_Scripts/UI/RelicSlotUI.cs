using System;
using UnityEngine;
using UnityEngine.UI;

public class RelicSlotUI : UIBase
{
    [SerializeField] private Image Icon;
    [SerializeField] private UIButton Button_Base;

    private RelicItem _relic;
    private RelicSlotUI _slotPrefab;
    private bool _isOwned;
    private Action<RelicItem, RelicSlotUI, bool> _onClickCallback;

    private static readonly Color OwnedColor = Color.white;
    private static readonly Color LockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

     private void OnEnable()
    {
        if (Button_Base != null)
        {
            Debug.Log($"[RelicSlotUI] {gameObject.name} Bind 시도, Button_Base null? {Button_Base == null}");

            Button_Base.BindOnClickButtonEvent(OnClickButton);
        }
    }

    public void SetIcon(Sprite sprite, bool isOwned)
    {
        if (Icon == null) return;

        Icon.sprite = sprite;
        Icon.color = isOwned ? OwnedColor : LockedColor;
        _isOwned = isOwned;
    }

    public void SetClickData(RelicItem relic, RelicSlotUI slotPrefab, Action<RelicItem, RelicSlotUI, bool> onClickCallback)
    {
        _relic = relic;
        _slotPrefab = slotPrefab;
        _onClickCallback = onClickCallback;
    }

    private void OnClickButton()
    {
        Debug.Log($"[RelicSlotUI] {gameObject.name} 클릭됨!");

        _onClickCallback?.Invoke(_relic, _slotPrefab, _isOwned);
    }
}