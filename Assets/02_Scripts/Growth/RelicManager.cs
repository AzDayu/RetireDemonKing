using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    [Header("=== 유물 도감 리스트 ===")]
    [SerializeField] private List<RelicItem> _relicList = new List<RelicItem>();

    public void Initialize()
    {
        // 유물 초기화
    }

    public Dictionary<StatType, float> GetTotalPercentStats()
    {
        var percentStatsMap = new Dictionary<StatType, float>();

        foreach (var relic in _relicList)
        {
            if (relic.Level > 0)
            {
                float currentBonus = relic.BasePercentBonus + ((relic.Level - 1) * relic.PercentBonusPerLevel);

                if (percentStatsMap.ContainsKey(relic.TargetStatType))
                {
                    percentStatsMap[relic.TargetStatType] += currentBonus;
                }
                else
                {
                    percentStatsMap[relic.TargetStatType] = currentBonus;
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
            Debug.LogWarning("[RelicManager] 모든 유물의 DropWeight 합이 0 이하입니다. 데이터 세팅을 확인하세요.");
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

        selectedRelic.Level++;

        Debug.Log($"[RelicManager] 유물 뽑기 성공! [{selectedRelic.Grade}] {selectedRelic.Name} (현재 Lv.{selectedRelic.Level})");

        GameManager.Instance.Growth.RecalculateTotalStats();

        return true;
    }
}
