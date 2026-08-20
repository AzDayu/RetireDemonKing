using UnityEngine;

public class GrowthPopupUI : UIBase
{
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private GameObject Popup_Growth;

    private void OnEnable()
    {
        if (Button_Close != null)
        {
            Button_Close.BindOnClickButtonEvent(OnClick_Close);
        }
    }

    private void OnDisable()
    {
        if (Button_Close != null)
        {
            Button_Close.UnBindAllOnClickButtonEvent();
        }
    }

    private void OnClick_Close()
    {
        if (Popup_Growth != null)
        {
            Popup_Growth.SetActive(false);
        }
    }
}
