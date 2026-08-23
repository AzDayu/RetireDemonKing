using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private Dictionary<string, RelicItem> _relicDataDict = new Dictionary<string, RelicItem>();
    private Dictionary<string, EquipmentItem> _equipmentDataDict = new Dictionary<string, EquipmentItem>();
    private Dictionary<string, MonsterData> _monsterDataDict = new Dictionary<string, MonsterData>();
    private Dictionary<string, MonsterData> _monsterDataByPrefabDict = new Dictionary<string, MonsterData>();

    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> items;
    }

    public void LoadAllData()
    {
        _relicDataDict = LoadData<RelicItem>("Relic", data => data.Id);
        _equipmentDataDict = LoadData<EquipmentItem>("Equipment", data => data.Id);

        Debug.Log($"[GameDataManager] 데이터 로드 완료 - 유물: {_relicDataDict.Count}개, 장비: {_equipmentDataDict.Count}개");
    }

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

    public MonsterData GetMonsterDataByPrefabName(string prefabName)
    {
        return _monsterDataByPrefabDict.TryGetValue(prefabName, out var data) ? data : null;
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
    // 테스트용 임시 데이터끝
}