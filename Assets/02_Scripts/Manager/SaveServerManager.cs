using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using RetireDemonKing.Network;

public class SaveServerManager : MonoBehaviour
{
    [Header("=== 서버 연동 옵션 ===")]
    [SerializeField] private bool _useServerSync = true;

    private PlayerSaveData _cachedSaveData;
    private string _localSavePath;

    private void Awake()
    {
        _localSavePath = Path.Combine(Application.persistentDataPath, "SaveData.dat");
    }

    public async Task<bool> LoadGameDataAsync()
    {
        bool isLoadedFromServer = false;

        if (_useServerSync && NetworkManager.Instance.IsLoggedIn) 
        {
            var tcs = new TaskCompletionSource<bool>();

            NetworkManager.Instance.RequestLoadSave((success, response) =>
            {
                if (success && response != null && !string.IsNullOrEmpty(response.saveJson))
                {
                    try
                    {
                        _cachedSaveData = JsonUtility.FromJson<PlayerSaveData>(response.saveJson);
                        _cachedSaveData.LastSaveUnixMinutes = response.lastSaveTicks;
                        isLoadedFromServer = true;
                        Debug.Log("[SaveServerManager] 서버 DB로부터 최신 세이브 데이터를 동기화했습니다.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SaveServerManager] 세이브 파싱 오류: {ex.Message}");
                    }
                }
                tcs.SetResult(isLoadedFromServer);
            });

            await tcs.Task;
        }

        if (!isLoadedFromServer)
        {
            Debug.LogWarning("[SaveServerManager] 서버 데이터를 불러오지 못해 로컬 세이브를 확인합니다.");
            _cachedSaveData = LoadLocalAES();
        }

        if (_cachedSaveData == null)
        {
            Debug.Log("[SaveServerManager] 세이브 데이터가 없어 신규 유저 기본 데이터를 생성합니다.");
            _cachedSaveData = CreateDefaultData();
            SaveGameData();
        }

        return true;
    }

    public void SaveGameData()
    {
        if (_cachedSaveData == null) return;

        _cachedSaveData.LastSaveUnixMinutes = GetCurrentUnixMinutes();
        string rawJson = JsonUtility.ToJson(_cachedSaveData, true);

        SaveLocalAES(rawJson);

        if (_useServerSync && NetworkManager.Instance.IsLoggedIn)
        {
            NetworkManager.Instance.RequestSyncSave(rawJson, _cachedSaveData.LastSaveUnixMinutes, (success, msg) =>
            {
                if (success)
                {
                    Debug.Log($"[SaveServerManager] 서버 동기화 완료 (저장 분: {_cachedSaveData.LastSaveUnixMinutes})");
                }
            });
        }
    }

    private long GetCurrentUnixMinutes()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;
    }

    private void SaveLocalAES(string json)
    {
        string encrypted = AESCryptoUtil.Encrypt(json);
        if (!string.IsNullOrEmpty(encrypted))
        {
            File.WriteAllText(_localSavePath, encrypted);
        }
    }

    private PlayerSaveData LoadLocalAES()
    {
        if (!File.Exists(_localSavePath)) return null;

        try
        {
            string encrypted = File.ReadAllText(_localSavePath);
            string decryptedJson = AESCryptoUtil.Decrypt(encrypted);
            return string.IsNullOrEmpty(decryptedJson) ? null : JsonUtility.FromJson<PlayerSaveData>(decryptedJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveServerManager] 로컬 복호화 실패: {ex.Message}");
            return null;
        }
    }

    private PlayerSaveData CreateDefaultData()
    {
        return new PlayerSaveData
        {
            Player = new PlayerModel
            {
                Level = 1,
                CurrentExp = 0,
                Gold = 0,
                EnhanceCurrency = 0,
                RebirthPoints = 0,
                CurrentStage = 1,
                MaxStage = 1
            },
            Equipments = new System.Collections.Generic.List<EquipmentModel>(),
            Relics = new System.Collections.Generic.List<RelicModel>(),
            LastSaveUnixMinutes = GetCurrentUnixMinutes(),
            UserAccountId = NetworkManager.Instance.CurrentUserAccountId
        };
    }

    public PlayerSaveData GetSaveData() => _cachedSaveData;
    public PlayerModel GetPlayerModel() => _cachedSaveData?.Player;
    public System.Collections.Generic.List<EquipmentModel> GetEquipments() => _cachedSaveData?.Equipments;
    public System.Collections.Generic.List<RelicModel> GetRelics() => _cachedSaveData?.Relics;
    public long GetLastSaveUnixMinutes() => _cachedSaveData?.LastSaveUnixMinutes ?? GetCurrentUnixMinutes();
}