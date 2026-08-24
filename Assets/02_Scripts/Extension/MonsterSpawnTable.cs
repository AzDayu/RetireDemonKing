using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterSpawnEntry
{
    public string monsterId;
    public int count;
}

[Serializable]
public class ThemeSpawnEntry
{
    public StageTheme theme;
    public List<MonsterSpawnEntry> monsters;   // 일반 웨이브용
    public string bossMonsterId;               // 보스전용 (추가)
}

[CreateAssetMenu(menuName = "Data/Monster Spawn Table")]
public class MonsterSpawnTable : ScriptableObject
{
    public List<ThemeSpawnEntry> entries;

    private Dictionary<StageTheme, ThemeSpawnEntry> _lookup;

    private void BuildLookupIfNeeded()
    {
        if (_lookup != null) return;

        _lookup = new Dictionary<StageTheme, ThemeSpawnEntry>();
        foreach (var entry in entries)
        {
            if (_lookup.ContainsKey(entry.theme))
            {
                Debug.LogWarning($"[MonsterSpawnTable] 테마 중복 등록: {entry.theme}");
                continue;
            }
            _lookup.Add(entry.theme, entry);
        }
    }

    public List<MonsterSpawnEntry> GetMonsters(StageTheme theme)
    {
        BuildLookupIfNeeded();
        return _lookup.TryGetValue(theme, out var entry) ? entry.monsters : new List<MonsterSpawnEntry>();
    }

    public string GetBossMonsterId(StageTheme theme)
    {
        BuildLookupIfNeeded();
        if (!_lookup.TryGetValue(theme, out var entry) || string.IsNullOrEmpty(entry.bossMonsterId))
        {
            Debug.LogWarning($"[MonsterSpawnTable] 테마 {theme}에 보스 몬스터가 지정되지 않았습니다.");
            return null;
        }
        return entry.bossMonsterId;
    }

#if UNITY_EDITOR
    private void OnValidate() => _lookup = null;
#endif
}