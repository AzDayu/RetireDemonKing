using System;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] private Button Button_Base;
    [SerializeField] private Text Text_Base;
    [SerializeField] private Image Image_Base;
    [SerializeField] private Image Image_Select;

    private bool _isSlotMenualUnbindEvent;

    private void Awake()
    {
        InitUIButton();
        SetDefaultUI();
    }

    private void OnDisable()
    {
        if (_isSlotMenualUnbindEvent == false)
        {
            Button_Base.onClick.RemoveAllListeners();
        }
    }


    private void SetDefaultUI()
    {
        if (Image_Select != null)
        {
            Image_Select.gameObject.SetActive(false);
        }
    }

    private void InitUIButton()
    {
        if (Button_Base != null)
        {
            return;
        }

        // 누락했다면 등록안해도 알아서 찾도록
        var button = this.gameObject.GetComponentInChildren<Button>();
        if (button != null)
        {
            this.Button_Base = button;
        }
    }

    public void BindOnClickButtonEvent(Action onClickCallback, bool isMenualUnbineEvent = false)
    {
        if (Button_Base == null) return;

        Button_Base.onClick.AddListener(onClickCallback.Invoke);
        _isSlotMenualUnbindEvent = isMenualUnbineEvent;
    }

    public void UnBindAllOnClickButtonEvent()
    {
        if (Button_Base == null) return;

        Button_Base.onClick.RemoveAllListeners();
    }

    public void ChangeButtonText(string buttonStr)
    {
        if (Text_Base == null) return;

        Text_Base.text = buttonStr;
    }
}