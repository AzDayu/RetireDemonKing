using System;

namespace RetireDemonKing.Network
{
    [Serializable]
    public class AuthRequest
    {
        public string accountId;
        public string password;

        public AuthRequest(string id, string pw)
        {
            accountId = id;
            password = pw;
        }
    }

    [Serializable]
    public class SaveSyncRequest
    {
        public string saveJson;
        public long clientTicks;

        public SaveSyncRequest(string json, long ticks)
        {
            saveJson = json;
            clientTicks = ticks;
        }
    }

    [Serializable]
    public class BaseResponse
    {
        public bool success;
        public string message;
    }

    [Serializable]
    public class LoginResponse : BaseResponse
    {
        public string token;
    }

    [Serializable]
    public class SaveLoadResponse : BaseResponse
    {
        public string saveJson;
        public long lastSaveTicks;
    }
}
