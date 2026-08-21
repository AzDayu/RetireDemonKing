using System.Collections.Generic;
using UnityEngine;

public static class StageManagerExtension
{
    public static readonly List<string> BgThemeKeys = new List<string>()
    {
        "forest1", "forest2",
        "nightForest1", "nightForest2",
        "desert1", "desert2",
        "ruin1", "passage1", "passage2",
        "sea1", "sea2", "sea3",
        "snow1", "snow2", "snow3", "snow4",
        "castle1",
    };

    
    public static string GetThemeAddressKey(this StageManager stageManager, int stageIndex, int stagesPerChange = 10)
    {
        if (BgThemeKeys == null || BgThemeKeys.Count == 0)
        {
            return string.Empty;
        }

        int themeIndex = (stageIndex - 1) / stagesPerChange;
        
        // 등록된 테마 개수를 초과하면 테마 유지 (루프 시 : % _bgThemeKeys.Count 사용)
        themeIndex = Mathf.Clamp(themeIndex, 0, BgThemeKeys.Count - 1);

        return BgThemeKeys[themeIndex];
    }
}