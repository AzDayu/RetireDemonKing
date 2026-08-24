using System;
using UnityEngine;

public enum OfflineManagerState
{
    NotInitalized, // 게임 시작
    Ready, // 저장 데이터 로드 및 초기화 완료
    RewardReady // 오프라인 보상 계산 완료
}


public class OfflineManager : MonoBehaviour
{
    [Header("오프라인 진행 설정")]
    [SerializeField, Min(0)]
    // 0 이하의 값을 세팅 할 수 없음

    private int _minimumOfflineMinutes = 1;
    // 오프라인 상태 진입 후 60초 지나야 오프라인 보상 계산 시작

    [SerializeField, Min(1)]
    // 1 이하의 값을 세팅 할 수 없음

    private int _maximumOfflineHours = 21;
    //  최대 21시간의 오프라인 보상을 수령할 수 있음

    private OfflineManagerState _currentState = OfflineManagerState.NotInitalized;

    private OfflineRewardSnapshot _loadedSnapshot;

    public OfflineManagerState CurrentState { get { return _currentState; } }
    
    public OfflineRewardResult PendingReward { get; private set; }
    // 계산 끝남 + 미수령 보상 보관


    public bool HasPendingReward => _currentState == OfflineManagerState.RewardReady && PendingReward != null;

    public event Action<OfflineRewardResult> RewardPrepared;
    // 오프라인 보상 계산 종료 알림 


    public event Action RewardClaimed;
    // 보상 수령 완료를 알림


    public event Action<OfflineRewardSnapshot> SnapshotCaptured;
    // 게임 종료시 오프라인 보상 관련 스냅샷 생성 알림



    public OfflineRewardResult CalculateReward(long currentUnixTime, OfflineRewardSnapshot snapshot)
    {
        long maximumOfflineMinutes = (long)_maximumOfflineHours * 60;

        return OfflineRewardCalculator.Calculate(
            snapshot,
            currentUnixTime,
            _minimumOfflineMinutes,
            maximumOfflineMinutes);
    }

    public void ProcessOfflineReward(long lastSaveUnixMinutes, int currentStage)
    {
        long currentUnixMinutes = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;

        double goldPerMin = currentStage * 60.0;
        double expPerMin = currentStage * 30.0;

        var snapshot = new OfflineRewardSnapshot
        {
            LastActiveUnixTime = lastSaveUnixMinutes,
            GoldPerMinute = goldPerMin,
            ExperiencePerMinute = expPerMin
        };

        PendingReward = CalculateReward(currentUnixMinutes, snapshot);

        if (PendingReward != null && PendingReward.ElapsedMinutes >= _minimumOfflineMinutes)
        {
            _currentState = OfflineManagerState.RewardReady;
            Debug.Log($"[OfflineManager] 오프라인 보상 계산 완료 - 방치: {PendingReward.ElapsedMinutes}분 (Gold: {PendingReward.Gold}, Exp: {PendingReward.Experience})");
            RewardPrepared?.Invoke(PendingReward);
        }
        else
        {
            _currentState = OfflineManagerState.Ready;
            Debug.Log("[OfflineManager] 오프라인 시간이 최소 기준치(1분) 미만입니다.");
        }
    }

    public void ClaimPendingReward(PlayerModel playerModel)
    {
        if (!HasPendingReward || playerModel == null) return;

        playerModel.Gold += PendingReward.Gold;

        if (GameManager.Instance != null && GameManager.Instance.Growth != null)
        {
            GameManager.Instance.Growth.AddExp(PendingReward.Experience);
        }

        Debug.Log($"[OfflineManager] 오프라인 보상 지급 완료 - Gold: {PendingReward.Gold}, Exp: {PendingReward.Experience}");

        PendingReward = null;
        _currentState = OfflineManagerState.Ready;
        RewardClaimed?.Invoke();

        GameManager.Instance.SaveServer?.SaveGameData();
    }
}

public static class OfflineRewardCalculator
{
    public static OfflineRewardResult Calculate(
        OfflineRewardSnapshot snapshot,
        long currentUnixTime,
        long minimumOfflineMinutes,
        long maximumOfflineMinutes)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }


        // 게임 첫 실행(이전 종료 기록 없음) & 현재 시각이 마지막 활동 시각보다 과거임
        if (snapshot.LastActiveUnixTime <= 0 || currentUnixTime <= snapshot.LastActiveUnixTime)
        {
            return new OfflineRewardResult();
        }

        long rawElapsedSeconds = currentUnixTime - snapshot.LastActiveUnixTime;

        long rawElapsedMinutes = rawElapsedSeconds / 60;


        // minimumOfflineMinutes가 음수인 경우 방지
        long safeMinimumOfflineMinutes = Math.Max(0, minimumOfflineMinutes);


        if (rawElapsedMinutes < safeMinimumOfflineMinutes)
        {
            return new OfflineRewardResult();
        }


        // 최대 오프라인 인정 시간이 최소 인정 시간보다 작아지지 않도록
        long safeMaximumOfflineMinutes =
            Math.Max(
                safeMinimumOfflineMinutes,
                maximumOfflineMinutes);

        // 오프라인 보상 인정 시간 제한
        long elapsedMinutes =
            Math.Min(
                rawElapsedMinutes,
                safeMaximumOfflineMinutes);

        double goldPerMinute = Math.Max(0, snapshot.GoldPerMinute);

        double experiencePerMinute = Math.Max(0, snapshot.ExperiencePerMinute);

        return new OfflineRewardResult
        {
            ElapsedMinutes = elapsedMinutes,

            Gold = (long)Math.Floor(elapsedMinutes * goldPerMinute),

            Experience = (long)Math.Floor(elapsedMinutes * experiencePerMinute),

            WasTimeCapped = rawElapsedMinutes > elapsedMinutes
        };
    }
}



[Serializable]
public class OfflineRewardSnapshot
{
    public long LastActiveUnixTime;
    public double GoldPerMinute;
    public double ExperiencePerMinute;
}

[Serializable]
public class OfflineRewardResult
{
    public long ElapsedMinutes;
    public long Gold;
    public long Experience;
    public bool WasTimeCapped;
}