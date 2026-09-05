using UnityEngine;
using UnityEngine.UI;


public class MainHUDUI : UIBase
{
    [SerializeField] private Button _buttonMenu;

    private void Awake()
    {
        InitUIButton();
    }

    private void InitUIButton()
    {
        if (_buttonMenu != null)
        {
            _buttonMenu.onClick.RemoveAllListeners();
            _buttonMenu.onClick.AddListener(OnClickMenu);
        }
    }

    private void OnClickMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.OpenMenuPopupUI();
        }
    }
}
