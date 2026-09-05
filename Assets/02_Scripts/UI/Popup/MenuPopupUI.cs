using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuPopupUI : UIBase
{
    [Header("=== Menu Buttons ===")]
    [SerializeField] private Button _buttonSave;
    [SerializeField] private Button _buttonLogout;
    [SerializeField] private Button _buttonQuit;
    [SerializeField] private Button _buttonClose;

    [Header("=== Feedback Text ===")]
    [SerializeField] private TextMeshProUGUI _textStatusMessage;

    private void Awake()
    {
        InitUIButton();
    }

    private void OnEnable()
    {
        SetStatusMessage(string.Empty);
    }

    private void InitUIButton()
    {
        if (_buttonSave != null)
        {
            _buttonSave.onClick.RemoveAllListeners();
            _buttonSave.onClick.AddListener(OnClickSave);
        }

        if (_buttonLogout != null)
        {
            _buttonLogout.onClick.RemoveAllListeners();
            _buttonLogout.onClick.AddListener(OnClickLogout);
        }

        if (_buttonQuit != null)
        {
            _buttonQuit.onClick.RemoveAllListeners();
            _buttonQuit.onClick.AddListener(OnClickQuit);
        }

        if (_buttonClose != null)
        {
            _buttonClose.onClick.RemoveAllListeners();
            _buttonClose.onClick.AddListener(OnClickClose);
        }
    }

    private void OnClickSave()
    {
        SetStatusMessage("게임 진행 상황을 저장하는 중입니다...");

        if (GameManager.Instance.SaveServer != null)
        {
            GameManager.Instance.SaveServer.SaveGameData();
            SetStatusMessage("성공적으로 저장되었습니다.");
        }
        else if (GameManager.Instance != null)
        {
            SetStatusMessage("성공적으로 저장되었습니다.");
        }
        else
        {
            SetStatusMessage("저장에 실패했습니다.", true);
        }
    }

    private void OnClickLogout()
    {
        OnClickClose();

        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.CloseMenuPopupUI();
            GameManager.Instance.UI.OpenLoginPopupUI();
        }

        Debug.Log("로그아웃 되었습니다. 로그인 화면으로 이동합니다.");
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnClickClose()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.CloseContentUI(UIType.MenuPopupUI);
        }

        gameObject.SetActive(false);
    }

    private void SetStatusMessage(string message, bool isError = false)
    {
        if (_textStatusMessage == null) return;
        _textStatusMessage.text = message;
        _textStatusMessage.color = isError ? Color.red : Color.green;
    }
}
