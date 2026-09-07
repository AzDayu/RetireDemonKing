using UnityEngine;
using Cysharp.Threading.Tasks;

public static class PlayerNetworkService
{
    private static PlayerModel _playerModel;

    public static void Initialize(PlayerModel playerModel)
    {
        _playerModel = playerModel;
    }

    public static UniTask AddGoldAsync(long amount)
    {
        if (_playerModel == null)
        {
            Debug.LogError("[PlayerNetworkService] PlayerModel이 초기화되지 않았습니다.");
            return UniTask.CompletedTask;
        }

        _playerModel.Gold += amount;
        return UniTask.CompletedTask;
    }
}