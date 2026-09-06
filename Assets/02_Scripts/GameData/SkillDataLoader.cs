using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillDataLoader
{
    private const string ResourcePath = "Skill";

    [Serializable]
    private class SerializationWrapper
    {
        public List<SkillItem> items = new List<SkillItem>();
    }

    private static readonly Dictionary<string, Dictionary<int, SkillItem>>
        SkillDataMap = new Dictionary<string, Dictionary<int, SkillItem>>();

    private static readonly Dictionary<string, int> DisplayOrderMap =
        new Dictionary<string, int>();

    private static readonly List<string> OrderedSkillIds =
        new List<string>();

    private static bool _isLoaded;

    public static bool Load()
    {
        if (_isLoaded)
        {
            return SkillDataMap.Count > 0;
        }

        TextAsset textAsset = Resources.Load<TextAsset>(ResourcePath);
        if (textAsset == null)
        {
            Debug.LogError(
                "[SkillDataLoader] JSON 파일을 찾지 못했습니다: " +
                "Resources/" + ResourcePath
            );
            return false;
        }

        SkillDataMap.Clear();
        DisplayOrderMap.Clear();
        OrderedSkillIds.Clear();

        try
        {
            string wrappedJson = "{\"items\":" + textAsset.text + "}";
            SerializationWrapper wrapper =
                JsonUtility.FromJson<SerializationWrapper>(wrappedJson);

            if (wrapper == null || wrapper.items == null)
            {
                Debug.LogError("[SkillDataLoader] 스킬 데이터가 비어 있습니다.");
                return false;
            }

            var itemIds = new HashSet<string>();

            foreach (SkillItem item in wrapper.items)
            {
                if (!IsValid(item))
                {
                    continue;
                }

                if (!itemIds.Add(item.Id))
                {
                    Debug.LogWarning(
                        "[SkillDataLoader] 중복된 Id를 건너뜁니다: " +
                        item.Id
                    );
                    continue;
                }

                if (!SkillDataMap.TryGetValue(
                        item.SkillId,
                        out Dictionary<int, SkillItem> levels))
                {
                    levels = new Dictionary<int, SkillItem>();
                    SkillDataMap.Add(item.SkillId, levels);
                    DisplayOrderMap.Add(item.SkillId, item.DisplayOrder);
                    OrderedSkillIds.Add(item.SkillId);
                }
                else
                {
                    DisplayOrderMap[item.SkillId] = Math.Min(
                        DisplayOrderMap[item.SkillId],
                        item.DisplayOrder
                    );
                }

                if (levels.ContainsKey(item.Level))
                {
                    Debug.LogWarning(
                        "[SkillDataLoader] 중복된 스킬 레벨을 건너뜁니다: " +
                        item.SkillId + " Lv." + item.Level
                    );
                    continue;
                }

                levels.Add(item.Level, item);
            }

            OrderedSkillIds.Sort(CompareSkillIds);
            _isLoaded = SkillDataMap.Count > 0;

            Debug.Log(
                "[SkillDataLoader] 스킬 데이터 로드 완료: " +
                SkillDataMap.Count + "개"
            );

            return _isLoaded;
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[SkillDataLoader] Skill.json 파싱 오류: " +
                ex.Message
            );
            return false;
        }
        finally
        {
            Resources.UnloadAsset(textAsset);
        }
    }

    public static List<string> GetSkillIdsInDisplayOrder()
    {
        return Load()
            ? new List<string>(OrderedSkillIds)
            : new List<string>();
    }

    public static SkillItem GetSkillData(string skillId, int level)
    {
        if (!Load() ||
            string.IsNullOrEmpty(skillId) ||
            !SkillDataMap.TryGetValue(
                skillId,
                out Dictionary<int, SkillItem> levels) ||
            !levels.TryGetValue(level, out SkillItem item))
        {
            return null;
        }

        return item;
    }

    public static SkillItem GetFirstSkillData(string skillId)
    {
        if (!Load() ||
            string.IsNullOrEmpty(skillId) ||
            !SkillDataMap.TryGetValue(
                skillId,
                out Dictionary<int, SkillItem> levels))
        {
            return null;
        }

        SkillItem first = null;

        foreach (SkillItem item in levels.Values)
        {
            if (first == null || item.Level < first.Level)
            {
                first = item;
            }
        }

        return first;
    }

    private static bool IsValid(SkillItem item)
    {
        if (item != null &&
            !string.IsNullOrWhiteSpace(item.Id) &&
            !string.IsNullOrWhiteSpace(item.SkillId) &&
            item.Level > 0)
        {
            return true;
        }

        Debug.LogWarning(
            "[SkillDataLoader] 필수 값이 없는 스킬 데이터를 건너뜁니다."
        );
        return false;
    }

    private static int CompareSkillIds(string left, string right)
    {
        int orderComparison =
            DisplayOrderMap[left].CompareTo(DisplayOrderMap[right]);

        return orderComparison != 0
            ? orderComparison
            : string.CompareOrdinal(left, right);
    }
}
