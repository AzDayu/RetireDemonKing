using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class RelicInfoPopupUI : UIBase
{
    [SerializeField] private Transform _iconContainer;
    [SerializeField] private TextMeshProUGUI Text_StatBonus;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_CloseAll;

    private void OnEnable()
    {
        Button_Close.BindOnClickButtonEvent(OnClickClose);
        Button_CloseAll.BindOnClickButtonEvent(OnClickClose);
    }

    public async UniTaskVoid Open(RelicItem relic, RelicSlotUI slotPrefab, bool isOwned)
    {
        gameObject.SetActive(true);

        foreach (Transform child in _iconContainer)
        {
            Destroy(child.gameObject);
        }

        RelicSlotUI iconInstance = Instantiate(slotPrefab, _iconContainer);
        Sprite sprite = await GameManager.Instance.Resource.LoadSprite(relic.IconId);
        iconInstance.SetIcon(sprite, isOwned);

        Text_StatBonus.text = $"{relic.TargetStatType} +{relic.BasePercentBonus}%";
        Text_Description.text = relic.Description;
    }

    private void OnClickClose()
    {
        GameManager.Instance.UI.ClosePopupUI(UIType.RelicInfoPopupUI);
    }
}