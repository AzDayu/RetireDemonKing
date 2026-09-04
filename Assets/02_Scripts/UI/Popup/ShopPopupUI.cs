using UnityEngine;
using System.Collections.Generic;



public class ShopPopupUI : UIBase
{
    private enum ChestTier
    {
        Low,
        High,
    }

    [Header("Buttons")]
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_EquipmentLowChest;
    [SerializeField] private UIButton Button_EquipmentHighChest;
    [SerializeField] private UIButton Button_RelicLowChest;
    [SerializeField] private UIButton Button_RelicHighChest;



    private void OnEnable()
    {
        Button_Close?.BindOnClickButtonEvent(OnClickClose);

        Button_EquipmentLowChest?.BindOnClickButtonEvent(() => OnClickEquipmentChest(ChestTier.Low));
        Button_EquipmentHighChest?.BindOnClickButtonEvent(() => OnClickEquipmentChest(ChestTier.High));

        Button_RelicLowChest?.BindOnClickButtonEvent(() => OnClickRelicChest(ChestTier.Low));
        Button_RelicHighChest?.BindOnClickButtonEvent(() => OnClickRelicChest(ChestTier.High));
    }

    private void OnDisable()
    {
        Button_Close?.UnBindAllOnClickButtonEvent();
        Button_EquipmentLowChest?.UnBindAllOnClickButtonEvent();
        Button_EquipmentHighChest?.UnBindAllOnClickButtonEvent();
        Button_RelicLowChest?.UnBindAllOnClickButtonEvent();
        Button_RelicHighChest?.UnBindAllOnClickButtonEvent();

    }

    private void OnClickClose()
    {
        GameManager.Instance.UI.ClosePopupUI(UIType.ShopPopupUI);
    }

    private void OnClickEquipmentChest(ChestTier tier)
    {
        List<EquipmentItem> equipmentList = GetAllEquipmentItems();

        List<EquipmentItem> candidates = GetEquipmentItemsByTier(equipmentList, tier);

        EquipmentItem selectedEquipment = GetRandomEquipment(candidates);


        if (selectedEquipment == null)
        {
            return;
        }

        Debug.Log(
            $"[ShopPopupUI] 장비 추첨 결과: " +
            $"[{selectedEquipment.Grade}] " +
            $"{selectedEquipment.Name} / " +
            $"ID: {selectedEquipment.Id}"
        );
    }

    private List<EquipmentItem> GetAllEquipmentItems()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager가 없습니다.");

            return new List<EquipmentItem>();
        }

        if (GameManager.Instance.Data == null)
        {
            Debug.LogWarning("GameDataManager가 없습니다.");

            return new List<EquipmentItem>();
        }

        List<EquipmentItem> equipmentList = GameManager.Instance.Data.GetAllEquipmentDataList();

        if (equipmentList == null)
        {
            Debug.LogWarning("장비 목록을 가져오지 못했습니다.");

            return new List<EquipmentItem>();
        }

        return equipmentList;
    }

    private List<EquipmentItem> GetEquipmentItemsByTier(List<EquipmentItem> equipmentList, ChestTier tier)
    {
        List<EquipmentItem> result = new List<EquipmentItem>();

        if (equipmentList == null)
        {
            return result;
        }

        foreach (EquipmentItem equipment in equipmentList)
        {
            if (equipment == null)
            {
                continue;
            }

            bool isTargetGrade;

            if (tier == ChestTier.Low)
            {
                isTargetGrade = equipment.Grade == EquipmentGrade.Common || equipment.Grade == EquipmentGrade.Rare;
            }


            else
            {
                isTargetGrade =
                    equipment.Grade == EquipmentGrade.Epic ||
                    equipment.Grade == EquipmentGrade.Legendary ||
                    equipment.Grade == EquipmentGrade.Mythic;
            }

            if (isTargetGrade)
            {
                result.Add(equipment);
            }
        }

        return result;
    }

    private EquipmentItem GetRandomEquipment(List<EquipmentItem> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("추첨 가능한 장비가 없습니다.");

            return null;
        }

        int totalWeight = 0;

        foreach (EquipmentItem equipment in candidates)
        {
            if (equipment == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0, equipment.DropWeight);
        }

        if (totalWeight <= 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            return candidates[randomIndex];
        }

        int randomValue = Random.Range(0, totalWeight);

        int accumulatedWeight = 0;

        foreach (EquipmentItem equipment in candidates)
        {
            if (equipment == null)
            {
                continue;
            }

            accumulatedWeight += Mathf.Max(0, equipment.DropWeight);

            if (randomValue < accumulatedWeight)
            {
                return equipment;
            }
        }

        return candidates[candidates.Count - 1];
    }

    private void OnClickRelicChest(ChestTier tier)
    {
        List<RelicItem> relicList = GetAllRelicItems();

        List<RelicItem> candidates =
            GetRelicItemsByTier(relicList, tier);

        RelicItem selectedRelic =
            GetRandomRelic(candidates);

        if (selectedRelic == null)
        {
            return;
        }

        Debug.Log(
            $"[ShopPopupUI] 렐릭 추첨 결과: " +
            $"상자 등급: {tier} / " +
            $"{selectedRelic.Name} / " +
            $"ID: {selectedRelic.Id}"
        );
    }

    private List<RelicItem> GetAllRelicItems()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager가 없습니다.");

            return new List<RelicItem>();
        }

        if (GameManager.Instance.Data == null)
        {
            Debug.LogWarning("GameDataManager가 없습니다.");

            return new List<RelicItem>();
        }

        List<RelicItem> relicList = GameManager.Instance.Data.GetAllRelicDataList();

        if (relicList == null)
        {
            Debug.LogWarning("렐릭 목록을 가져오지 못했습니다.");

            return new List<RelicItem>();
        }

        return relicList;
    }

    private List<RelicItem> GetRelicItemsByTier(List<RelicItem> relicList, ChestTier tier)
    {
        List<RelicItem> result = new List<RelicItem>();

        if (relicList == null)
        {
            return result;
        }

        foreach (RelicItem relic in relicList)
        {
            if (relic == null)
            {
                continue;
            }

            bool isTargetGrade;
            if (tier == ChestTier.Low)
            {
                isTargetGrade = relic.Id.EndsWith("_01") || relic.Id.EndsWith("_02");
            }

            else
            {
                isTargetGrade = relic.Id.EndsWith("_03") || relic.Id.EndsWith("_04");
            }

            if (isTargetGrade)
            {
                result.Add(relic);
            }
        }

        return result;
    }

    private RelicItem GetRandomRelic(List<RelicItem> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("추첨 가능한 렐릭이 없습니다.");

            return null;
        }

        int totalWeight = 0;

        foreach (RelicItem relic in candidates)
        {
            if (relic == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0, relic.DropWeight);
        }

        if (totalWeight <= 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            return candidates[randomIndex];
        }

        int randomValue = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        foreach (RelicItem relic in candidates)
        {
            if (relic == null)
            {
                continue;
            }

            accumulatedWeight += Mathf.Max(0, relic.DropWeight);

            if (randomValue < accumulatedWeight)
            {
                return relic;
            }
        }

        return candidates[candidates.Count - 1];
    }


}