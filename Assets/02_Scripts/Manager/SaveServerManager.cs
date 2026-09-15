using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using RetireDemonKing.Network;
using System.Security.Cryptography;
using System.Text;


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

    private string GetLocalSavePath(string accountId)
    {
        string source = string.IsNullOrEmpty(accountId)? "guest": accountId;

        string accountKey;

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            byte[] hash = sha256.ComputeHash(bytes);

            accountKey = BitConverter.ToString(hash).Replace("-", string.Empty).Substring(0, 16);
        }

        return Path.Combine(Application.persistentDataPath,$"SaveData_{accountKey}.dat");
    }

    public async Task<bool> LoadGameDataAsync()
    {
        string accountId = NetworkManager.Instance.CurrentUserAccountId;

        bool attemptedServerLoad = _useServerSync && NetworkManager.Instance.IsLoggedIn;

        bool isLoadedFromServer = false;

        _cachedSaveData = null;

        if (_useServerSync && NetworkManager.Instance.IsLoggedIn) 
        {
            var tcs = new TaskCompletionSource<bool>();

            NetworkManager.Instance.RequestLoadSave((success, response) =>
            {
                if (success && response != null && !string.IsNullOrEmpty(response.saveJson))
                {
                    try
                    {
                        PlayerSaveData loadedData = JsonUtility.FromJson<PlayerSaveData>(response.saveJson);

                        bool isSameAccount =loadedData != null && (string.IsNullOrEmpty
                        (loadedData.UserAccountId) || string.Equals(
                                 loadedData.UserAccountId,
                                 accountId,
                                 StringComparison.Ordinal)
                        );

                        if (isSameAccount)
                        {
                            loadedData.UserAccountId = accountId;
                            loadedData.LastSaveUnixMinutes = response.lastSaveTicks;

                            NormalizeSaveData(loadedData);

                            _cachedSaveData = loadedData;
                            isLoadedFromServer = true;
                        }
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
            _cachedSaveData = LoadLocalAES(accountId);
        }

        if (_cachedSaveData == null)
        {
            if (attemptedServerLoad)
            {
                Debug.LogError(
                    "[SaveServerManager] 서버와 현재 계정의 로컬 세이브를 " + "모두 불러오지 못했습니다. 기본 데이터로 덮어쓰지 않습니다.");
                return false;
            }

            Debug.Log("[SaveServerManager] 신규 유저 기본 데이터를 생성합니다.");
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

        SaveLocalAES(rawJson,_cachedSaveData.UserAccountId);

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

    private bool SaveLocalAES(string json, string accountId)
    {
        try
        {
            string encrypted = AESCryptoUtil.Encrypt(json);
            if (string.IsNullOrEmpty(encrypted))
            {
                return false;
            }

            string savePath = GetLocalSavePath(accountId);
            File.WriteAllText(savePath, encrypted);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveServerManager] 로컬 저장 실패: {ex.Message}");
            return false;
        }
    }

    private PlayerSaveData LoadLocalAES(string accountId)
    {
        string savePath = GetLocalSavePath(accountId);

        if (!File.Exists(savePath))
        {
            string legacyPath = Path.Combine(Application.persistentDataPath,"SaveData.dat");

            if (!File.Exists(legacyPath))
            {
                return null;
            }

            savePath = legacyPath;
        }

        try
        {
            string encrypted = File.ReadAllText(savePath);
            string decryptedJson = AESCryptoUtil.Decrypt(encrypted);

            if (string.IsNullOrEmpty(decryptedJson))
            {
                return null;
            }

            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(decryptedJson);

            if (saveData == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(accountId) && string.IsNullOrEmpty(saveData.UserAccountId))
            {
                Debug.LogWarning(
                    "[SaveServerManager] 계정 정보가 없는 로컬 세이브를 무시합니다.");
                return null;
            }

            if (!string.Equals(saveData.UserAccountId,accountId,StringComparison.Ordinal))
            {
                Debug.LogWarning("[SaveServerManager] 다른 계정의 로컬 세이브를 차단했습니다.");
                return null;
            }

            NormalizeSaveData(saveData);
            return saveData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveServerManager] 로컬 복호화 실패: {ex.Message}");
            return null;
        }
    }

    private void NormalizeSaveData(PlayerSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.Player ??= new PlayerModel();
        saveData.Equipments ??= new System.Collections.Generic.List<EquipmentModel>();
        saveData.Relics ??= new System.Collections.Generic.List<RelicModel>();
        saveData.Skills ??= new System.Collections.Generic.List<SkillModel>();

        saveData.Player.CurrentStage = Math.Max(1, saveData.Player.CurrentStage);

        saveData.Player.MaxStage = Math.Max(saveData.Player.CurrentStage,saveData.Player.MaxStage);
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