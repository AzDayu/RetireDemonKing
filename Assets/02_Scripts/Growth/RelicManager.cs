using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    private List<RelicItem> _relicList = new List<RelicItem>();

    private Dictionary<string, RelicModel> _ownedRelicMap = new Dictionary<string, RelicModel>();

    public void Initialize(List<RelicModel> savedRelicList)
    {
        _ownedRelicMap.Clear();

        if (savedRelicList != null)
        {
            foreach (var model in savedRelicList)
            {
                _ownedRelicMap[model.RelicId] = model;
            }
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
        if (playerModel == null || playerModel.RebirthPoints < cost || _relicList == null || _relicList.Count == 0)
            return false;

        int totalWeight = 0;
        foreach (var relic in _relicList)
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

        foreach (var relic in _relicList)
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
            selectedRelic = _relicList[0];
        }

        // 유물 데이터 갱신
        if (!_ownedRelicMap.TryGetValue(selectedRelic.Id, out RelicModel model))
        {
            model = new RelicModel { RelicId = selectedRelic.Id, Level = 0 };
            _ownedRelicMap[selectedRelic.Id] = model;
        }

        model.Level++;

        Debug.Log($"[RelicManager] 유물 뽑기 성공! [{selectedRelic.Grade}] {selectedRelic.Name} (현재 Lv.{model.Level})");

        GameManager.Instance.Growth.RecalculateTotalStats();

        return true;
    }

    public List<RelicModel> GetSavedRelicList()
    {
        return new List<RelicModel>(_ownedRelicMap.Values);
    }
}