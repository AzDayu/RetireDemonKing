using System.Collections.Generic;
using UnityEngine;

public class GameUtil
{
    public static int GetThemeFirstStage(int stageIndex, int stagesForChange)
    {
        int themeIndex = (int)stageIndex.GetTheme(stagesForChange);
        return themeIndex * stagesForChange + 1;
    }

    // Fisher-Yates 셔플
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
        { 
            return default; 
        }
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    // 등급별 조회 후 장비 드랍
    public static EquipmentGrade RollGrade(DropTableData dropTable)
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += dropTable.CommonDropRate;
        if (roll <= cumulative) return EquipmentGrade.Common;

        cumulative += dropTable.RareDropRate;
        if (roll <= cumulative) return EquipmentGrade.Rare;

        cumulative += dropTable.EpicDropRate;
        if (roll <= cumulative) return EquipmentGrade.Epic;

        cumulative += dropTable.LegendaryDropRate;
        if (roll <= cumulative) return EquipmentGrade.Legendary;

        return EquipmentGrade.Mythic;
    }

    public static EquipmentItem GetRandomEquipmentByGrade(
        EquipmentGrade grade,
        List<EquipmentItem> allEquipmentData)
    {
        List<EquipmentItem> candidates = new List<EquipmentItem>();

        foreach (EquipmentItem item in allEquipmentData)
        {
            if (item.Grade == grade)
            {
                candidates.Add(item);
            }
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }
}
