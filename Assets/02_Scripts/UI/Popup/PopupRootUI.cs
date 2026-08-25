using UnityEngine;

public class GrowthPopupUI : UIBase
{
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private GameObject Popup_root;

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
        if (Popup_root != null)
        {
            Popup_root.SetActive(false);
        }
    }
}
