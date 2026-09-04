using UnityEngine;
using UnityEngine.UI;

public class RelicSlotUI : UIBase
{
    [SerializeField] private Image Icon;

    private static readonly Color OwnedColor = Color.white;
    private static readonly Color LockedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    public void SetIcon(Sprite sprite, bool isOwned)
    {
        if (Icon == null) return;

        Icon.sprite = sprite;
        Icon.color = isOwned ? OwnedColor : LockedColor;
    }
}