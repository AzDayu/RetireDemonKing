using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicChestResultPanelUI : MonoBehaviour
{
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _guide;
    private Button _confirmButton;
    private bool _isInitialized;

    public bool IsVisible => gameObject.activeSelf;

    private void Awake()
    {
        Initialize();
    }

    public void Show(RelicDrawResult result)
    {
        if (!Initialize() || result.Relic == null)
        {
            return;
        }

        _title.text = result.IsNew
            ? "새 유물 획득"
            : "유물 레벨 상승";
        _guide.text =
            $"[{GetGradeDisplayName(result.Relic.Grade)}] " +
            $"{result.Relic.Name}\n\n" +
            $"현재 레벨: Lv.{result.Level}\n\n" +
            result.Relic.Description;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        _title = FindText("Panel/Title");
        _guide = FindText("Panel/Guide");
        _confirmButton = FindButton("Panel/EquipButton");

        if (_title == null ||
            _guide == null ||
            _confirmButton == null)
        {
            Debug.LogError(
                "[RelicChestResultPanelUI] 결과창 프리팹 참조를 찾지 못했습니다."
            );
            return false;
        }

        SetDefaultFont(_title);
        SetDefaultFont(_guide);

        _confirmButton.onClick.AddListener(Hide);
        _isInitialized = true;
        gameObject.SetActive(false);
        return true;
    }

    private TextMeshProUGUI FindText(string path)
    {
        Transform textTransform = transform.Find(path);
        return textTransform != null
            ? textTransform.GetComponent<TextMeshProUGUI>()
            : null;
    }

    private Button FindButton(string path)
    {
        Transform buttonTransform = transform.Find(path);
        return buttonTransform != null
            ? buttonTransform.GetComponent<Button>()
            : null;
    }

    private void SetDefaultFont(TextMeshProUGUI text)
    {
        if (text.font == null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }
    }

    private string GetGradeDisplayName(EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.Common:
                return "일반";
            case EquipmentGrade.Rare:
                return "고급";
            case EquipmentGrade.Epic:
                return "희귀";
            case EquipmentGrade.Legendary:
                return "전설";
            case EquipmentGrade.Mythic:
                return "신화";
            default:
                return grade.ToString();
        }
    }
}
