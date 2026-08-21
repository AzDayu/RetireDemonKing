using RetireDemonKing.Network;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RetireDemonKing.Network
{
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("[NetworkManager]");
                    _instance = obj.AddComponent<NetworkManager>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        [SerializeField] private string _baseUrl = "http://localhost:3000/api";

        private string _jwtToken = string.Empty;
        private string _currentUserAccountId = string.Empty;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_jwtToken);
        public string CurrentUserAccountId => _currentUserAccountId;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void RequestRegister(string accountId, string password, Action<bool, string> onComplete)
        {
            var reqDto = new AuthRequest(accountId, password);
            string jsonBody = JsonUtility.ToJson(reqDto);

            StartCoroutine(PostRoutine($"{_baseUrl}/auth/register", jsonBody, false, (success, responseText) =>
            {
                if (success)
                {
                    var res = JsonUtility.FromJson<BaseResponse>(responseText);
                    onComplete?.Invoke(res.success, res.message);
                }
                else
                {
                    onComplete?.Invoke(false, responseText);
                }
            }));
        }

        public void RequestLogin(string accountId, string password, Action<bool, string> onComplete)
        {
            var reqDto = new AuthRequest(accountId, password);
            string jsonBody = JsonUtility.ToJson(reqDto);

            StartCoroutine(PostRoutine($"{_baseUrl}/auth/login", jsonBody, false, (success, responseText) =>
            {
                if (success)
                {
                    var res = JsonUtility.FromJson<LoginResponse>(responseText);
                    if (res.success)
                    {
                        _jwtToken = res.token;
                        _currentUserAccountId = accountId;
                        Debug.Log($"[NetworkManager] 로그인 성공! 계정: {accountId}");
                    }
                    onComplete?.Invoke(res.success, res.message);
                }
                else
                {
                    onComplete?.Invoke(false, responseText);
                }
            }));
        }

        public void RequestSyncSave(string saveJson, long clientTicks, Action<bool, string> onComplete)
        {
            if (!IsLoggedIn)
            {
                onComplete?.Invoke(false, "로그인이 필요합니다.");
                return;
            }

            var reqDto = new SaveSyncRequest(saveJson, clientTicks);
            string jsonBody = JsonUtility.ToJson(reqDto);

            StartCoroutine(PostRoutine($"{_baseUrl}/save/sync", jsonBody, true, (success, responseText) =>
            {
                if (success)
                {
                    var res = JsonUtility.FromJson<BaseResponse>(responseText);
                    onComplete?.Invoke(res.success, res.message);
                }
                else
                {
                    onComplete?.Invoke(false, responseText);
                }
            }));
        }

        public void RequestLoadSave(Action<bool, SaveLoadResponse> onComplete)
        {
            if (!IsLoggedIn)
            {
                onComplete?.Invoke(false, null);
                return;
            }

            StartCoroutine(GetRoutine($"{_baseUrl}/save/load", true, (success, responseText) =>
            {
                if (success)
                {
                    var res = JsonUtility.FromJson<SaveLoadResponse>(responseText);
                    onComplete?.Invoke(res.success, res);
                }
                else
                {
                    onComplete?.Invoke(false, null);
                }
            }));
        }

        private IEnumerator PostRoutine(string url, string jsonBody, bool includeAuth, Action<bool, string> callback)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (includeAuth && !string.IsNullOrEmpty(_jwtToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, request.downloadHandler.text);
                }
                else
                {
                    string errorMsg = string.IsNullOrEmpty(request.downloadHandler.text)
                        ? request.error
                        : request.downloadHandler.text;
                    callback?.Invoke(false, errorMsg);
                }
            }
        }

        private IEnumerator GetRoutine(string url, bool includeAuth, Action<bool, string> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (includeAuth && !string.IsNullOrEmpty(_jwtToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, request.downloadHandler.text);
                }
                else
                {
                    callback?.Invoke(false, request.error);
                }
            }
        }
    }
}