using UnityEngine;

public class BottomBarUI : UIBase
{
    [SerializeField] private UIButton Button_Growth;
    [SerializeField] private UIButton Button_Skill;
    [SerializeField] private UIButton Button_Shop;
    [SerializeField] private UIButton Button_Rebirth;

    [SerializeField] private GameObject Popup_Root;
    [SerializeField] private GameObject Popup_Growth;
    [SerializeField] private GameObject Popup_Skill;
    [SerializeField] private GameObject Popup_Shop;
    [SerializeField] private GameObject Popup_Rebirth;

    private void OnEnable()
    {
        Button_Growth.BindOnClickButtonEvent(OnClick_Growth);
        Button_Skill.BindOnClickButtonEvent(OnClick_Skill);
        Button_Shop.BindOnClickButtonEvent(OnClick_Shop);
        Button_Rebirth.BindOnClickButtonEvent(OnClick_Rebirth);
    }

    private void OnDisable()
    {
        Button_Growth.UnBindAllOnClickButtonEvent();
        Button_Skill.UnBindAllOnClickButtonEvent();
        Button_Shop.UnBindAllOnClickButtonEvent();
        Button_Rebirth.UnBindAllOnClickButtonEvent();
    }

    private void OnClick_Growth() => ShowPopup(Popup_Growth);
    private void OnClick_Skill() => ShowPopup(Popup_Skill);
    private void OnClick_Shop() => ShowPopup(Popup_Shop);
    private void OnClick_Rebirth() => ShowPopup(Popup_Rebirth);

    private void ShowPopup(GameObject targetPopup)
    {
        Popup_Growth.SetActive(targetPopup == Popup_Growth);
        Popup_Skill.SetActive(targetPopup == Popup_Skill);
        Popup_Shop.SetActive(targetPopup == Popup_Shop);
        Popup_Rebirth.SetActive(targetPopup == Popup_Rebirth);

        Popup_Root.SetActive(true);
    }
}