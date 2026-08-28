using UnityEngine;

public class PopupRootUI : UIBase
{
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private GameObject Popup_Equipment;
    [SerializeField] private GameObject Popup_Skill;

    private void OnEnable()
    {
        if (Button_Close != null)
        {
            Button_Close.BindOnClickButtonEvent(OnClickClose);
        }
    }

    private void OnDisable()
    {
        if (Button_Close != null)
        {
            Button_Close.UnBindAllOnClickButtonEvent();
        }
    }

    public void ShowEquipment()
    {
        ShowPopup(Popup_Equipment);
    }

    public void ShowSkill()
    {
        ShowPopup(Popup_Skill);
    }

    private void ShowPopup(GameObject target)
    {
        if (Popup_Equipment != null)
        {
            Popup_Equipment.SetActive(target == Popup_Equipment);
        }

        if (Popup_Skill != null)
        {
            Popup_Skill.SetActive(target == Popup_Skill);
        }
    }

    private void OnClickClose()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.ClosePopupUI(UIType.PopupRootUI);
        }
    }
}