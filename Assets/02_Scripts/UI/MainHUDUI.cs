using UnityEngine;
using UnityEngine.UI;


public class MainHUDUI : UIBase
{
    [SerializeField] private Button _buttonMenu;
    [SerializeField] private Button _buttonRebirth;

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
        if (_buttonRebirth != null)
        {
            _buttonRebirth.onClick.RemoveAllListeners();
            _buttonRebirth.onClick.AddListener(OnClickRebirth);
        }
    }

    private void OnClickMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.OpenMenuPopupUI();
        }
    }
    private void OnClickRebirth()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.OpenRebirthPopupUI();
        }
    }
}
