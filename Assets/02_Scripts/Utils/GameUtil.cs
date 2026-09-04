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

    
}
