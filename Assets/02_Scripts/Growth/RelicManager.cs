using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    private List<RelicItem> _relicList = new List<RelicItem>();

    private Dictionary<string, RelicModel> _ownedRelicMap = new Dictionary<string, RelicModel>();
    private List<RelicModel> _savedRelicList = new List<RelicModel>();

    public void Initialize(List<RelicModel> savedRelicList)
    {
        _relicList = GameManager.Instance != null &&
            GameManager.Instance.Data != null
                ? GameManager.Instance.Data.GetAllRelicDataList()
                : new List<RelicItem>();
        _savedRelicList = savedRelicList ?? new List<RelicModel>();
        _ownedRelicMap.Clear();

        foreach (RelicModel model in _savedRelicList)
        {
            if (model == null ||
                string.IsNullOrEmpty(model.RelicId) ||
                model.Level <= 0)
            {
                continue;
            }

            _ownedRelicMap[model.RelicId] = model;
        }
    }

    public Dictionary<StatType, float> GetTotalPercentStats()
    {
        var percentStatsMap = new Dictionary<StatType, float>();

        foreach (var relicStatic in _relicList)
        {
            if (_ownedRelicMap.TryGetValue(relicStatic.Id, out RelicModel relicModel) && relicModel.Level > 0)
            {
                float currentBonus = relicStatic.BasePercentBonus + ((relicModel.Level - 1) * relicStatic.PercentBonusPerLevel);

                if (percentStatsMap.ContainsKey(relicStatic.TargetStatType))
                {
                    percentStatsMap[relicStatic.TargetStatType] += currentBonus;
                }
                else
                {
                    percentStatsMap[relicStatic.TargetStatType] = currentBonus;
                }
            }
        }

        return percentStatsMap;
    }

    public bool TryDrawRelicWithRebirthPoints(PlayerModel playerModel, int cost)
    {
        return TryDrawRelic(
            playerModel,
            cost,
            new[]
            {
                EquipmentGrade.Common,
                EquipmentGrade.Rare,
                EquipmentGrade.Epic,
                EquipmentGrade.Legendary
            },
            out _
        );
    }

    public bool TryDrawRelic(
        PlayerModel playerModel,
        int cost,
        EquipmentGrade[] availableGrades,
        out RelicDrawResult result)
    {
        result = default;

        if (playerModel == null ||
            cost < 0 ||
            playerModel.RebirthPoints < cost ||
            _relicList == null ||
            _relicList.Count == 0 ||
            availableGrades == null ||
            availableGrades.Length == 0)
        {
            return false;
        }

        var candidates = new List<RelicItem>();

        foreach (RelicItem relic in _relicList)
        {
            if (relic == null || relic.DropWeight <= 0)
            {
                continue;
            }

            foreach (EquipmentGrade grade in availableGrades)
            {
                if (relic.Grade == grade)
                {
                    candidates.Add(relic);
                    break;
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[RelicManager] 조건에 맞는 유물이 없습니다.");
            return false;
        }

        int totalWeight = 0;
        foreach (RelicItem relic in candidates)
        {
            totalWeight += relic.DropWeight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("[RelicManager] 모든 유물의 DropWeight 합이 0 이하입니다.");
            return false;
        }

        playerModel.RebirthPoints -= cost;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeightSum = 0;
        RelicItem selectedRelic = null;

        foreach (RelicItem relic in candidates)
        {
            currentWeightSum += relic.DropWeight;
            if (randomValue < currentWeightSum)
            {
                selectedRelic = relic;
                break;
            }
        }

        if (selectedRelic == null)
        {
            selectedRelic = candidates[candidates.Count - 1];
        }

        bool isNew = !_ownedRelicMap.TryGetValue(
            selectedRelic.Id,
            out RelicModel model
        );

        if (isNew)
        {
            model = new RelicModel { RelicId = selectedRelic.Id, Level = 0 };
            _ownedRelicMap[selectedRelic.Id] = model;
            _savedRelicList.Add(model);
        }

        model.Level++;

        Debug.Log($"[RelicManager] 유물 뽑기 성공! [{selectedRelic.Grade}] {selectedRelic.Name} (현재 Lv.{model.Level})");

        GameManager.Instance.Growth.RecalculateTotalStats();
        result = new RelicDrawResult(selectedRelic, isNew, model.Level);

        return true;
    }

    public bool IsRelicOwned(string relicId)
    {
        return !string.IsNullOrEmpty(relicId) &&
            _ownedRelicMap.TryGetValue(
                relicId,
                out RelicModel model
            ) &&
            model.Level > 0;
    }

    public int GetRelicLevel(string relicId)
    {
        return IsRelicOwned(relicId)
            ? _ownedRelicMap[relicId].Level
            : 0;
    }

    public List<RelicModel> GetSavedRelicList()
    {
        return new List<RelicModel>(_ownedRelicMap.Values);
    }
}

public readonly struct RelicDrawResult
{
    public readonly RelicItem Relic;
    public readonly bool IsNew;
    public readonly int Level;

    public RelicDrawResult(RelicItem relic, bool isNew, int level)
    {
        Relic = relic;
        IsNew = isNew;
        Level = level;
    }
}
