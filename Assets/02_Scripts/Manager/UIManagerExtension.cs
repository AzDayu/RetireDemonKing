using UnityEngine;

public enum UIRootType
{
    None = 0,
    BackGroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

public enum UIType
{
    StartTitleUI,
    LoadingUI,
    InventoryUI,
    LoginPopupUI,
    MainHUDUI,
    PopupRootUI,
    RandomEventPopupUI,
    StageProgressUI,
    BossTimerUI,
    BossHudUI,
    RelicUI,

}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty;

        path = $"{uiRootType}/{uiType}";
        return path;
    }

    public static void ShowStartupUIOnGameStart(this UIManager uiManager)
    {
        //uiManager.OpenUI(UIRootType.MainUI, UIType.RelicUI);
    }


    public static void OpenLoadingUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
        if (uiBase == null)
        {
            Debug.LogWarning("UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    public static void OpenLoginPopupUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoginPopupUI);
       
    }
    public static void CloseLoginPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.LoginPopupUI);
    }

    public static void OpenMainHUDUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.MainUI, UIType.MainHUDUI);

    }
    public static void CloseMainHUDUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.MainHUDUI);
    }

    public static void OpenRandomEventPopupUI(this UIManager uiManager, RandomEventStaticData eventData)
    {
        var uiBase = uiManager.OpenPopupUI(UIType.RandomEventPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning("[UIManager] RandomEventPopupUI 생성 실패!");
            return;
        }

        if (uiBase is RandomEventPopupUI popupUI)
        {
            popupUI.SetUI(eventData);
        }
    }

    public static void CloseRandomEventPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.RandomEventPopupUI);
    }

    public static void OpenPopupRootUI(this UIManager uiManager)
    {
        uiManager.OpenUI(UIRootType.PopupUI, UIType.PopupRootUI);

    }
    public static void ClosePopupRootUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.PopupRootUI);
    }
}
