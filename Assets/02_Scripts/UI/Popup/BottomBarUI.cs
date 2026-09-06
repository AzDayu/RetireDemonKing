using UnityEngine;

public class BottomBarUI : UIBase
{
    [SerializeField] private UIButton Button_Growth;
    [SerializeField] private UIButton Button_Skill;
    [SerializeField] private UIButton Button_Shop;
    [SerializeField] private UIButton Button_Relic;

    private void OnEnable()
    {
        Button_Growth?.BindOnClickButtonEvent(OnClick_Growth);
        Button_Skill?.BindOnClickButtonEvent(OnClick_Skill);
        Button_Relic?.BindOnClickButtonEvent(OnClick_Relic);
        Button_Shop?.BindOnClickButtonEvent(OnClick_Shop);
    }

    private void OnDisable()
    {
        Button_Growth?.UnBindAllOnClickButtonEvent();
        Button_Skill?.UnBindAllOnClickButtonEvent();
        Button_Shop?.UnBindAllOnClickButtonEvent();
        Button_Relic?.UnBindAllOnClickButtonEvent();
    }

    private void OnClick_Growth()
    {
        GameManager.Instance.UI.OpenPopupUI(UIType.GrowthPopupUI);
    }

    private void OnClick_Skill()
    {
        GameManager.Instance.UI.OpenPopupUI(UIType.SkillPopupUI);
    }
    private void OnClick_Relic()
    {
        GameManager.Instance.UI.OpenPopupUI(UIType.RelicUI);
    }

    private void OnClick_Shop()
    {
        GameManager.Instance.UI.OpenPopupUI(UIType.ShopPopupUI);
    }

}