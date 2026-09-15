using UnityEngine;

public class RebirthManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _minRebirthStage = 10;

    public int MinRebirthStage => _minRebirthStage;


    public bool CanRebirth(int currentStage)
    {
        return currentStage >= _minRebirthStage;
    }

    public int CalculateRebirthPoints(int currentStage, float rebirthPointBonusPercent)
    {
        if (currentStage < _minRebirthStage)
            return 0;

        float basePoints = currentStage;
        float bonusMultiplier = 1f + (rebirthPointBonusPercent / 100f);

        int finalPoints = Mathf.FloorToInt(basePoints * bonusMultiplier);
        return Mathf.Max(1, finalPoints);
    }

    public bool TryExecuteRebirth(PlayerModel playerModel, int currentStage, float rebirthPointBonusPercent)
    {
        if (playerModel == null || !CanRebirth(currentStage))
        {
            Debug.LogWarning("[RebirthManager] 환생 조건을 만족하지 못했거나 PlayerModel이 null입니다.");
            return false;
        }

        int earnedPoints = CalculateRebirthPoints(currentStage, rebirthPointBonusPercent);
        playerModel.RebirthPoints += earnedPoints;

        Debug.Log($"[RebirthManager] 환생 성공! 획득 환생 포인트: {earnedPoints} (현재 총 보유: {playerModel.RebirthPoints})");

        ResetPlayerData(playerModel);

        return true;
    }

    private void ResetPlayerData(PlayerModel playerModel)
    {
        SaveServerManager saveServer = GameManager.Instance.SaveServer;
        PlayerSaveData saveData = saveServer?.GetSaveData();

        if (playerModel == null || saveData == null)
        {
            Debug.LogError("[RebirthManager] 환생 초기화에 필요한 저장 데이터가 없습니다.");
            return;
        }

        playerModel.Level = 1;
        playerModel.CurrentExp = 0;
        playerModel.Gold = 0;
        playerModel.CurrentStage = 1;
        playerModel.MaxStage = 1;

        EquipmentDropFlow.CancelPendingDrops();

        saveData.Equipments = saveServer.CreateStarterEquipments();

        saveServer.SaveGameData();
        GameManager.Instance.RestartInGameLoop();
    }
}