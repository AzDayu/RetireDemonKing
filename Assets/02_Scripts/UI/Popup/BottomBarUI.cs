using UnityEngine;

public class BottomBarUI : UIBase
{
    [SerializeField] private UIButton Button_Growth;
    [SerializeField] private UIButton Button_Skill;
    [SerializeField] private UIButton Button_Shop;

    private void OnEnable()
    {
        Button_Growth?.BindOnClickButtonEvent(OnClick_Growth);
        Button_Skill?.BindOnClickButtonEvent(OnClick_Skill);
        Button_Shop?.BindOnClickButtonEvent(OnClick_Shop);
    }

    private void OnDisable()
    {
        Button_Growth?.UnBindAllOnClickButtonEvent();
        Button_Skill?.UnBindAllOnClickButtonEvent();
        Button_Shop?.UnBindAllOnClickButtonEvent();
    }

    private void OnClick_Growth()
    {
        PopupRootUI popup = OpenPopupRoot();

        if (popup != null)
        {
            popup.ShowEquipment();
        }
    }

    private void OnClick_Skill()
    {
        PopupRootUI popup = OpenPopupRoot();

        if (popup != null)
        {
            popup.ShowSkill();
        }
    }

    private void OnClick_Shop()
    {
        if (GameManager.Instance == null || GameManager.Instance.UI == null)
        {
            Debug.LogWarning("[BottomBarUI] UIManager를 찾을 수 없습니다.");

            return;
        }

        GameManager.Instance.UI.OpenPopupUI(UIType.ShopPopupUI);
    }

    private PopupRootUI OpenPopupRoot()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.UI == null)
        {
            Debug.LogWarning(
                "[BottomBarUI] UIManager를 찾을 수 없습니다."
            );

            return null;
        }

        UIBase popup = GameManager.Instance.UI.OpenPopupUI(UIType.PopupRootUI);

        if (popup is PopupRootUI popupRoot)
        {
            return popupRoot;
        }

        Debug.LogWarning("[BottomBarUI] PopupRootUI를 생성하지 못했습니다.");

        return null;
    }
}