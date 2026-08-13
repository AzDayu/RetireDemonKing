using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private Dictionary<string, RelicItem> _relicDataDict = new Dictionary<string, RelicItem>();
    private Dictionary<string, EquipmentItem> _equipmentDataDict = new Dictionary<string, EquipmentItem>();

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
}