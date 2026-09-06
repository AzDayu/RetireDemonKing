using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class RelicScrollUI : UIBase
{
    [Header("등급별 슬롯 프리팹")]
    [SerializeField] private RelicSlotUI _commonSlotPrefab;
    [SerializeField] private RelicSlotUI _rareSlotPrefab;
    [SerializeField] private RelicSlotUI _epicSlotPrefab;
    [SerializeField] private RelicSlotUI _legendarySlotPrefab;

    [SerializeField] private Transform _content;
    [SerializeField] private UIButton Button_Close;

    private GameObject _backgroundOverlay;

    private void Awake()
    {
        CreateBackgroundOverlay();
    }

    private void OnEnable()
    {
        SetBackgroundOverlayActive(true);
        Button_Close?.BindOnClickButtonEvent(OnClickClose);
        BuildRelicList().Forget();
    }

    private void OnDisable()
    {
        SetBackgroundOverlayActive(false);
        Button_Close?.UnBindAllOnClickButtonEvent();
    }

    private void OnDestroy()
    {
        if (_backgroundOverlay != null)
        {
            Destroy(_backgroundOverlay);
        }
    }

    private void CreateBackgroundOverlay()
    {
        if (_backgroundOverlay != null || transform.parent == null)
        {
            return;
        }

        GameObject overlay = new GameObject(
            "RelicPopupBackdrop",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        overlay.layer = gameObject.layer;

        RectTransform rectTransform = overlay.GetComponent<RectTransform>();
        rectTransform.SetParent(transform.parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.4f);

        Button button = overlay.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(OnClickClose);

        overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());
        overlay.SetActive(false);
        _backgroundOverlay = overlay;
    }

    private void SetBackgroundOverlayActive(bool isActive)
    {
        if (_backgroundOverlay == null)
        {
            CreateBackgroundOverlay();
        }

        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.SetActive(isActive);
        }
    }

    private void OnClickClose()
    {
        GameManager.Instance.UI.ClosePopupUI(UIType.RelicUI);
    }

    public async UniTaskVoid BuildRelicList()
    {
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }

        List<RelicItem> allRelics = GameManager.Instance.Data.GetAllRelicDataList();
        Dictionary<StatType, List<RelicItem>> groupedByStat = new Dictionary<StatType, List<RelicItem>>();

        foreach (RelicItem relic in allRelics)
        {
            Debug.Log($"[RelicScrollUI] Id: {relic.Id}, Grade: {relic.Grade}, Stat: {relic.TargetStatType}");

            if (!groupedByStat.ContainsKey(relic.TargetStatType))
            {
                groupedByStat[relic.TargetStatType] = new List<RelicItem>();
            }
            groupedByStat[relic.TargetStatType].Add(relic);
        }

        foreach (KeyValuePair<StatType, List<RelicItem>> pair in groupedByStat)
        {
            List<RelicItem> group = pair.Value;
            await CreateSlot(group, EquipmentGrade.Common, _commonSlotPrefab);
            await CreateSlot(group, EquipmentGrade.Rare, _rareSlotPrefab);
            await CreateSlot(group, EquipmentGrade.Epic, _epicSlotPrefab);
            await CreateSlot(group, EquipmentGrade.Legendary, _legendarySlotPrefab);
        }
    }

    private async UniTask CreateSlot(IEnumerable<RelicItem> group, EquipmentGrade grade, RelicSlotUI slotPrefab)
    {
        RelicItem relic = null;
        foreach (RelicItem item in group)
        {
            if (item.Grade == grade)
            {
                relic = item;
                break;
            }
        }

        if (relic == null)
        {
            Debug.LogWarning($"[RelicScrollUI] {grade} 등급 유물이 없습니다.");
            return;
        }

        RelicSlotUI slotInstance = Instantiate(slotPrefab, _content);
        Sprite sprite = await GameManager.Instance.Resource.LoadSprite(relic.IconId);

        RelicManager relicManager =
            FindFirstObjectByType<RelicManager>();
        bool isOwned = relicManager != null &&
            relicManager.IsRelicOwned(relic.Id);
        slotInstance.SetIcon(sprite, isOwned);
        slotInstance.SetClickData(relic, slotPrefab, OnClickSlot);
    }
    private void OnClickSlot(RelicItem relic, RelicSlotUI slotPrefab, bool isOwned)
    {
        var popup = GameManager.Instance.UI.OpenPopupUI(UIType.RelicInfoPopupUI) as RelicInfoPopupUI;
        if (popup != null)
        {
            popup.Open(relic, slotPrefab, isOwned).Forget();
        }
    }
}
