using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageTheme
{
    Forest1, Forest2,
    NightForest1, NightForest2,
    Desert1, Desert2,
    Ruin1, Passage1, Passage2,
    Sea1, Sea2, Sea3,
    Snow1, Snow2, Snow3, Snow4,
    Castle1,
}

public static class StageManagerExtension
{
    private static readonly Dictionary<StageTheme, string> ThemeAddressKeys = new()
    {
        { StageTheme.Forest1, "forest1" },
        { StageTheme.Forest2, "forest2" },
        { StageTheme.NightForest1, "nightForest1" },
        { StageTheme.NightForest2, "nightForest2" },
        { StageTheme.Desert1, "desert1" },
        { StageTheme.Desert2, "desert2" },
        { StageTheme.Ruin1, "ruin1" },
        { StageTheme.Passage1, "passage1" },
        { StageTheme.Passage2, "passage2" },
        { StageTheme.Sea1, "sea1" },
        { StageTheme.Sea2, "sea2" },
        { StageTheme.Sea3, "sea3" },
        { StageTheme.Snow1, "snow1" },
        { StageTheme.Snow2, "snow2" },
        { StageTheme.Snow3, "snow3" },
        { StageTheme.Snow4, "snow4" },
        { StageTheme.Castle1, "castle1" },
    };

    public static StageTheme GetTheme(this int stageIndex, int stagesPerChange)
    {
        int themeIndex = (stageIndex - 1) / stagesPerChange;
        int maxIndex = Enum.GetValues(typeof(StageTheme)).Length - 1;
        return (StageTheme)Mathf.Clamp(themeIndex, 0, maxIndex);
    }

    public static string GetThemeAddressKey(this StageManager stageManager, int stageIndex, int stagesPerChange)
    {
        StageTheme theme = stageIndex.GetTheme(stagesPerChange);
        return ThemeAddressKeys.TryGetValue(theme, out var key) ? key : string.Empty;
    }
}
