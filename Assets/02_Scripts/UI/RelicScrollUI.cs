using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class RelicScrollUI : UIBase
{
    [Header("등급별 슬롯 프리팹")]
    [SerializeField] private RelicSlotUI _commonSlotPrefab;
    [SerializeField] private RelicSlotUI _rareSlotPrefab;
    [SerializeField] private RelicSlotUI _epicSlotPrefab;
    [SerializeField] private RelicSlotUI _legendarySlotPrefab;

    [SerializeField] private Transform _content;
    [SerializeField] private UIButton Button_Close;

    private void OnEnable()
    {
        if (Button_Close != null)
        {
            Button_Close.BindOnClickButtonEvent(OnClickClose);
        }
        BuildRelicList().Forget();
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
        Debug.Log($"[RelicScrollUI] 유물 개수: {allRelics.Count}");

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

        // RelicManager에 공개 획득 여부 조회 메서드(IsRelicOwned) 추가 후 교체
        bool isOwned = false;
        slotInstance.SetIcon(sprite, isOwned);
    }
}