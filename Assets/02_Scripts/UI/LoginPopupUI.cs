using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RetireDemonKing.Network;

public class LoginPopupUI : UIBase
{
    [Header("=== Input Fields ===")]
    [SerializeField] private TMP_InputField Input_ID;
    [SerializeField] private TMP_InputField Input_Password;

    [Header("=== Buttons ===")]
    [SerializeField] private Button Button_Login;
    [SerializeField] private Button Button_Register;

    [Header("=== Feedback Text ===")]
    [SerializeField] private TextMeshProUGUI Text_StatusMessage;

    private void Awake()
    {
        InitUIButton();
    }

    private void OnEnable()
    {
        ClearInputs();
    }

    private void InitUIButton()
    {
        if (Button_Login != null)
        {
            Button_Login.onClick.RemoveAllListeners();
            Button_Login.onClick.AddListener(OnClickLogin);
        }

        if (Button_Register != null)
        {
            Button_Register.onClick.RemoveAllListeners();
            Button_Register.onClick.AddListener(OnClickRegister);
        }
    }

    private void ClearInputs()
    {
        if (Input_ID != null) Input_ID.text = string.Empty;
        if (Input_Password != null) Input_Password.text = string.Empty;
        SetStatusMessage(string.Empty);
    }

    private void SetStatusMessage(string message, bool isError = false)
    {
        if (Text_StatusMessage == null) return;
        Text_StatusMessage.text = message;
        Text_StatusMessage.color = isError ? Color.red : Color.green;
    }

    private void OnClickRegister()
    {
        string id = Input_ID.text.Trim();
        string pw = Input_Password.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            SetStatusMessage("아이디와 비밀번호를 모두 입력해 주세요.", true);
            return;
        }

        SetStatusMessage("회원가입 진행 중...");
        Button_Register.interactable = false;

        NetworkManager.Instance.RequestRegister(id, pw, (success, message) =>
        {
            Button_Register.interactable = true;
            if (success)
            {
                SetStatusMessage("회원가입 성공! 이제 로그인해 주세요.");
            }
            else
            {
                SetStatusMessage($"회원가입 실패: {message}", true);
            }
        });
    }

    private void OnClickLogin()
    {
        string id = Input_ID.text.Trim();
        string pw = Input_Password.text.Trim();

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            SetStatusMessage("아이디와 비밀번호를 모두 입력해 주세요.", true);
            return;
        }

        SetStatusMessage("로그인 중...");
        Button_Login.interactable = false;

        NetworkManager.Instance.RequestLogin(id, pw, (success, message) =>
        {
            Button_Login.interactable = true;
            if (success)
            {
                SetStatusMessage("로그인 성공! 게임 데이터를 불러옵니다.");
                OnLoginSuccess();
            }
            else
            {
                SetStatusMessage($"로그인 실패: {message}", true);
            }
        });
    }

    private void OnLoginSuccess()
    {
        if (GameManager.Instance != null && GameManager.Instance.UI != null)
        {
            GameManager.Instance.UI.CloseLoginPopupUI();
            GameManager.Instance.UI.OpenMainUI(UIType.MainHUDUI);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLoginSuccessAndStartGame();
        }
    }
}