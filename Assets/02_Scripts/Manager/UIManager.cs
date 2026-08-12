using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas Canvas_BgRoot;
    [SerializeField] Canvas Canvas_MainRoot;
    [SerializeField] Canvas Canvas_ContentRoot;
    [SerializeField] Canvas Canvas_PopupRoot;
    [SerializeField] Canvas Canvas_VeryFrontRoot;

    // 얘는 생성과 제거에 관한 부분 -> Instancing과 가비지컬렉터와 연관이 있는 애
    private Dictionary<UIType, UIBase> _createdUIDic = new Dictionary<UIType, UIBase>();
    // 얘는 활성과 비활성에 관한 부분 -> SetActive
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();


    private void Start()
    {
        this.ShowStartupUIOnGameStart();
    }

    public UIBase OpenUI(UIRootType uiRootType, UIType uiType, bool isInitialHide = false)
    {
        var openedUI = GetCreatedUI(uiRootType, uiType);

        if (openedUI == null)
        {
            Debug.LogError($"[UIManager] 에러 발생. {uiType} 프리팹을 로드하지 못했거나 UIBase 상속 스크립트가 없습니다.");
            return null;
        }

        bool isSetActiveOnOpen = (isInitialHide == false); // 열었을 때 기본적으로 숨겨서 열 것인지 체크
        if (_openedUIDic.Contains(uiType) == false)
        {
            openedUI.gameObject.SetActive(isSetActiveOnOpen);
            _openedUIDic.Add(uiType);
        }

        return openedUI;
    }

    private UIBase GetCreatedUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiRootType, uiType);
        }
        return _createdUIDic[uiType];
    }

    private void CreateUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = this.GetUIPath(uiRootType, uiType);
            GameObject loadedObj = Resources.Load<GameObject>(path);

            Transform root = GetRootTransform(uiRootType);
            GameObject gObj = Instantiate(loadedObj, root, false);
            if (gObj != null)
            {
                var uiBase = gObj.GetComponent<UIBase>();
                _createdUIDic.Add(uiType, uiBase);
            }
        }
    }

    public void CloseUI(UIRootType uiRootType, UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var openedUi = _createdUIDic[uiType];
            openedUi.gameObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }

    private Transform GetRootTransform(UIRootType uiRootType)
    {
        Transform root = null;
        switch (uiRootType)
        {
            case UIRootType.BackGroundUI:
                root = Canvas_BgRoot.transform;
                break;
            case UIRootType.MainUI:
                root = Canvas_MainRoot.transform;
                break;
            case UIRootType.ContentUI:
                root = Canvas_ContentRoot.transform;
                break;
            case UIRootType.PopupUI:
                root = Canvas_PopupRoot.transform;
                break;
            case UIRootType.VeryFrontUI:
                root = Canvas_VeryFrontRoot.transform;
                break;
        }
        return root;
    }

    // --------------------------------------------------------------

    public UIBase OpenBackgroundUI(UIType uiType)
    {
        return OpenUI(UIRootType.BackGroundUI, uiType);
    }
    public void CloseBackgroundUI(UIType uiType)
    {
        CloseUI(UIRootType.BackGroundUI, uiType);
    }

    public UIBase OpenMainUI(UIType uiType)
    {
        return OpenUI(UIRootType.MainUI, uiType);
    }
    public void CloseMainUI(UIType uiType)
    {
        CloseUI(UIRootType.MainUI, uiType);
    }

    public UIBase OpenContentUI(UIType uiType)
    {
        return OpenUI(UIRootType.ContentUI, uiType);
    }
    public void CloseContentUI(UIType uiType)
    {
        CloseUI(UIRootType.ContentUI, uiType);
    }

    public UIBase OpenPopupUI(UIType uiType)
    {
        return OpenUI(UIRootType.PopupUI, uiType);
    }
    public void ClosePopupUI(UIType uiType)
    {
        CloseUI(UIRootType.PopupUI, uiType);
    }

    public UIBase OpenVeryFrontUI(UIType uiType)
    {
        return OpenUI(UIRootType.VeryFrontUI, uiType);
    }
    public void CloseVeryFrontUI(UIType uiType)
    {
        CloseUI(UIRootType.VeryFrontUI, uiType);
    }
}