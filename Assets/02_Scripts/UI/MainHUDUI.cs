using UnityEngine;
using UnityEngine.UI;


public class MainHUDUI : UIBase
{
    [SerializeField] private Button _buttonMenu;
    [SerializeField] private Button _buttonRebirth;
    [SerializeField] private Slider _playerHpSlider;
    [SerializeField] private TMPro.TMP_Text _playerHpText;

    private PlayerController _playerController;

    private void Awake()
    {
        InitUIButton();
    }

    private void OnEnable()
    {
        BindPlayerHealth();
    }

    private void OnDisable()
    {
        UnbindPlayerHealth();
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

    private void BindPlayerHealth()
    {
        UnbindPlayerHealth();
        _playerController = PlayerController.Instance;

        if (_playerController == null)
        {
            RefreshPlayerHealth(0f, 0f);
            return;
        }

        _playerController.OnHpChanged += RefreshPlayerHealth;
        RefreshPlayerHealth(
            _playerController.CurHp,
            _playerController.MaxHp
        );
    }

    private void UnbindPlayerHealth()
    {
        if (_playerController != null)
        {
            _playerController.OnHpChanged -= RefreshPlayerHealth;
            _playerController = null;
        }
    }

    private void RefreshPlayerHealth(float currentHp, float maxHp)
    {
        float clampedMaxHp = Mathf.Max(0f, maxHp);
        float clampedCurrentHp = Mathf.Clamp(
            currentHp,
            0f,
            clampedMaxHp
        );

        if (_playerHpSlider != null)
        {
            _playerHpSlider.value = clampedMaxHp > 0f
                ? clampedCurrentHp / clampedMaxHp
                : 0f;
        }

        if (_playerHpText != null)
        {
            _playerHpText.text =
                $"{Mathf.CeilToInt(clampedCurrentHp):N0}/" +
                $"{Mathf.CeilToInt(clampedMaxHp):N0}";
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
