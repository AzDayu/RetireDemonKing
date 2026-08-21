using UnityEngine;

public class BottomBarUI : UIBase
{
    [SerializeField] private UIButton Button_Growth;


    [SerializeField] private GameObject Popup_Growth;


    private void OnEnable()
    {
        Button_Growth.BindOnClickButtonEvent(OnClick_Growth);
    }

    private void OnDisable()
    {
        Button_Growth.UnBindAllOnClickButtonEvent();
    }

    private void OnClick_Growth()
    {
        Popup_Growth.SetActive(true);
        // GameManager.Instance.UI.OpenPopupUI(UIType.GrowthPopupUI);
    }
}