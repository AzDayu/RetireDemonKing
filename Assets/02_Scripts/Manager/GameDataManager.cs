using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private Dictionary<string, RelicItem> _relicDataDict = new Dictionary<string, RelicItem>();
    private Dictionary<string, EquipmentItem> _equipmentDataDict = new Dictionary<string, EquipmentItem>();
    private Dictionary<string, MonsterData> _monsterDataDict = new Dictionary<string, MonsterData>();
    private Dictionary<string, RandomEventStaticData> _randomEventDataDict = new Dictionary<string, RandomEventStaticData>();


    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> items;
    }

    public void LoadAllData()
    {
        //_relicDataDict = LoadData<RelicItem>("Relic", data => data.Id);
        _relicDataDict = LoadRelicData();
        //_equipmentDataDict = LoadData<EquipmentItem>("Equipment", data => data.Id);
        _equipmentDataDict = LoadEquipmentData();
        _monsterDataDict = LoadData<MonsterData>("Monster", data => data.MonsterId);
        _randomEventDataDict = LoadData<RandomEventStaticData>("RandomEvent", data => data.Id);

        Debug.Log($"[GameDataManager] 데이터 로드 완료 - 유물: {_relicDataDict.Count}개, 장비: {_equipmentDataDict.Count}개. 몬스터: {_monsterDataDict.Count}개,이벤트:{_randomEventDataDict.Count}개");
    }

    // JsonUtility가 리플렉션으로 채우는 DTO 필드입니다.
#pragma warning disable CS0649
    [Serializable]
    private class EquipmentItemJson
    {
        public string Id;
        public string Name;
        public string Type;
        public string Grade;
        public string MainStatType;
        public float BaseStatValue;
        public float StatValuePerLevel;
        public float GradeMultiplier = 1f;
        public int DropWeight;
        public string PrefabId;
        public string IconId;
        public string Description;
    }
#pragma warning restore CS0649


    private Dictionary<string, T> LoadData<T>(string fileName, Func<T, string> keySelector)
    {
        string resourcePath = $"{fileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"[GameDataManager] JSON 파일 찾기 실패: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        var dict = new Dictionary<string, T>();

        try
        {
            string wrappedJson = "{\"items\":" + textAsset.text + "}";
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                foreach (var item in wrapper.items)
                {
                    string key = keySelector(item);
                    if (string.IsNullOrEmpty(key)) continue;

                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, item);
                    }
                    else
                    {
                        Debug.LogWarning($"[GameDataManager] {fileName} 내 중복된 ID 발견: {key} (기존 데이터 유지)");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameDataManager] {fileName} JSON 파싱 에러: {ex.Message}");
        }
        finally
        {
            Resources.UnloadAsset(textAsset);
        }

        return dict;
    }

    private Dictionary<string, EquipmentItem> LoadEquipmentData()
    {
        Dictionary<string, EquipmentItemJson> jsonDataDict =
            LoadData<EquipmentItemJson>(
                "Equipment",
                data => data.Id
            );

        var equipmentDataDict =
            new Dictionary<string, EquipmentItem>();

        foreach (EquipmentItemJson jsonData in
                 jsonDataDict.Values)
        {
            if (!Enum.TryParse(
                    jsonData.Type,
                    true,
                    out EquipmentType equipmentType) ||
                !Enum.TryParse(
                    jsonData.Grade,
                    true,
                    out EquipmentGrade equipmentGrade) ||
                !Enum.TryParse(
                    jsonData.MainStatType,
                    true,
                    out StatType mainStatType))
            {
                Debug.LogError(
                    $"[GameDataManager] 장비 enum 파싱 실패: " +
                    $"{jsonData.Id}"
                );

                continue;
            }

            equipmentDataDict.Add(
                jsonData.Id,
                new EquipmentItem
                {
                    Id = jsonData.Id,
                    Name = jsonData.Name,
                    Type = equipmentType,
                    Grade = equipmentGrade,
                    MainStatType = mainStatType,
                    BaseStatValue = jsonData.BaseStatValue,
                    StatValuePerLevel = jsonData.StatValuePerLevel,
                    GradeMultiplier = jsonData.GradeMultiplier,
                    DropWeight = jsonData.DropWeight,
                    PrefabId = jsonData.PrefabId,
                    IconId = jsonData.IconId,
                    Description = jsonData.Description
                }
            );
        }

        return equipmentDataDict;
    }

#pragma warning disable CS0649
    [Serializable]
    private class RelicItemJson
    {
        public string Id;
        public string Name;
        public string TargetStatType;
        public float BasePercentBonus;
        public float PercentBonusPerLevel;
        public string Grade;
        public int DropWeight;
        public string IconId;
        public string Description;
    }
#pragma warning restore CS0649

    private Dictionary<string, RelicItem> LoadRelicData()
    {
        Dictionary<string, RelicItemJson> jsonDataDict =
            LoadData<RelicItemJson>("Relic", data => data.Id);

        var relicDataDict = new Dictionary<string, RelicItem>();

        foreach (RelicItemJson jsonData in jsonDataDict.Values)
        {
            if (!Enum.TryParse(jsonData.TargetStatType, true, out StatType targetStatType) ||
                !Enum.TryParse(jsonData.Grade, true, out EquipmentGrade grade))
            {
                Debug.LogError($"[GameDataManager] 유물 enum 파싱 실패: {jsonData.Id}");
                continue;
            }

            relicDataDict.Add(
                jsonData.Id,
                new RelicItem
                {
                    Id = jsonData.Id,
                    Name = jsonData.Name,
                    TargetStatType = targetStatType,
                    BasePercentBonus = jsonData.BasePercentBonus,
                    PercentBonusPerLevel = jsonData.PercentBonusPerLevel,
                    Grade = grade,
                    DropWeight = jsonData.DropWeight,
                    IconId = jsonData.IconId,
                    Description = jsonData.Description
                }
            );
        }

        return relicDataDict;
    }

    public RelicItem GetRelicData(string id)
    {
        return _relicDataDict.TryGetValue(id, out var data) ? data : null;
    }

    public List<RelicItem> GetAllRelicDataList()
    {
        return _relicDataDict.Values.ToList();
    }

    public EquipmentItem GetEquipmentData(string id)
    {
        return _equipmentDataDict.TryGetValue(id, out var data) ? data : null;
    }

    public List<EquipmentItem> GetAllEquipmentDataList()
    {
        return _equipmentDataDict.Values.ToList();
    }

    public MonsterData GetMonsterData(string monsterId)
    {
        return _monsterDataDict.TryGetValue(monsterId, out var data) ? data : null;
    }

    // 테스트용 임시 데이터
    [ContextMenu("Test Equipment Data")]
    private void TestEquipmentData()
    {
        LoadAllData();

        EquipmentItem commonChest = GetEquipmentData("EQ_CHEST_ICE_Common");

        EquipmentItem rareChest = GetEquipmentData("EQ_CHEST_ICE_RARE");

        if (commonChest == null)
        {
            Debug.LogError("[장비 데이터 테스트] Common 상의 데이터를 찾지 못했습니다.");
        }
        else
        {
            Debug.Log(
                $"[장비 데이터 테스트] " +
                $"Id: {commonChest.Id}, " +
                $"Name: {commonChest.Name}, " +
                $"Type: {commonChest.Type}, " +
                $"Grade: {commonChest.Grade}, " +
                $"Stat: {commonChest.MainStatType}, " +
                $"Base: {commonChest.BaseStatValue}"
            );
        }

        if (rareChest == null)
        {
            Debug.LogError("[장비 데이터 테스트] Rare 상의 데이터를 찾지 못했습니다.");
        }
        else
        {
            Debug.Log(
                $"[장비 데이터 테스트] " +
                $"Id: {rareChest.Id}, " +
                $"Type: {rareChest.Type}, " +
                $"Grade: {rareChest.Grade}"
            );
        }
    }
    public RandomEventStaticData GetRandomEventData(string id)
    {
        return _randomEventDataDict.TryGetValue(id, out var data) ? data : null;
    }

    public List<RandomEventStaticData> GetAllRandomEventDataList()
    {
        return _randomEventDataDict.Values.ToList();
    }
}