using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("=== 이벤트 발생 설정 ===")]
    [SerializeField] private float _eventIntervalMin = 180f;
    [SerializeField] private float _eventIntervalMax = 300f;

    private float _nextEventTimer;
    private float _currentTimer;
    private bool _isTimerRunning = false;

    private List<ActiveBuff> _activeBuffList = new List<ActiveBuff>();

    public event Action<RandomEventStaticData> OnEventTriggered;
    public event Action OnBuffUpdated;

    public void Initialize()
    {
        ResetNextTimer();
        _isTimerRunning = true;
    }

    private void Update()
    {
        if (!_isTimerRunning) return;

        _currentTimer += Time.deltaTime;
        if (_currentTimer >= _nextEventTimer)
        {
            _currentTimer = 0f;
            ResetNextTimer();
            TriggerRandomEvent();
        }

        UpdateBuffs(Time.deltaTime);
    }

    private void ResetNextTimer()
    {
        _nextEventTimer = UnityEngine.Random.Range(_eventIntervalMin, _eventIntervalMax);
    }

    [ContextMenu("Test Trigger Event")]
    public void TriggerRandomEvent()
    {
        var eventList = GameManager.Instance.Data.GetAllRandomEventDataList();
        if (eventList == null || eventList.Count == 0) return;

        int totalWeight = 0;
        foreach (var evt in eventList) totalWeight += evt.Weight;

        if (totalWeight <= 0) return;

        int randomVal = UnityEngine.Random.Range(0, totalWeight);
        int currentSum = 0;
        RandomEventStaticData selectedEvent = null;

        foreach (var evt in eventList)
        {
            currentSum += evt.Weight;
            if (randomVal < currentSum)
            {
                selectedEvent = evt;
                break;
            }
        }

        if (selectedEvent != null)
        {
            Debug.Log($"[EventManager] 돌발 이벤트 발생: {selectedEvent.Title}");

            if (GameManager.Instance != null && GameManager.Instance.UI != null)
            {
                GameManager.Instance.UI.OpenRandomEventPopupUI(selectedEvent);
            }

            OnEventTriggered?.Invoke(selectedEvent);
        }

        Debug.Log($"[EventManager] 돌발 이벤트 발생: {selectedEvent.Title}");
        OnEventTriggered?.Invoke(selectedEvent);
    }

    public void SelectChoice1_Gold(RandomEventStaticData eventData)
    {
        if (eventData == null) return;

        int currentStage = GameManager.Instance.SaveServer.GetPlayerModel().CurrentStage;
        long baseMinuteGold = currentStage * 100L;
        long rewardGold = (long)(baseMinuteGold * eventData.GoldStageMultiplier);

        var player = GameManager.Instance.SaveServer.GetPlayerModel();
        if (player != null)
        {
            player.Gold += rewardGold;
            Debug.Log($"[EventManager] 선택지 1 골드 수령: +{rewardGold:N0} Gold");
        }
    }

    public void SelectChoice2_Buff(RandomEventStaticData eventData)
    {
        if (eventData == null) return;

        var existBuff = _activeBuffList.Find(b => b.TargetStat == eventData.BuffStatType);
        if (existBuff != null)
        {
            existBuff.PercentValue = Mathf.Max(existBuff.PercentValue, eventData.BuffPercent);
            existBuff.RemainingSeconds = eventData.BuffDurationSec;
        }
        else
        {
            _activeBuffList.Add(new ActiveBuff(eventData.BuffStatType, eventData.BuffPercent, eventData.BuffDurationSec));
        }

        Debug.Log($"[EventManager] 선택지 2 버프 활성화: {eventData.BuffStatType} +{eventData.BuffPercent}% ({eventData.BuffDurationSec}초)");

        GameManager.Instance.Growth.RecalculateTotalStats();
        OnBuffUpdated?.Invoke();
    }

    private void UpdateBuffs(float deltaTime)
    {
        if (_activeBuffList.Count == 0) return;

        bool isExpiredAny = false;
        for (int i = _activeBuffList.Count - 1; i >= 0; i--)
        {
            _activeBuffList[i].RemainingSeconds -= deltaTime;
            if (_activeBuffList[i].RemainingSeconds <= 0f)
            {
                Debug.Log($"[EventManager] 버프 만료: {_activeBuffList[i].TargetStat}");
                _activeBuffList.RemoveAt(i);
                isExpiredAny = true;
            }
        }

        if (isExpiredAny)
        {
            GameManager.Instance.Growth.RecalculateTotalStats();
            OnBuffUpdated?.Invoke();
        }
    }

    public Dictionary<StatType, float> GetTotalBuffPercentStats()
    {
        var map = new Dictionary<StatType, float>();
        foreach (var buff in _activeBuffList)
        {
            if (map.ContainsKey(buff.TargetStat)) map[buff.TargetStat] += buff.PercentValue;
            else map[buff.TargetStat] = buff.PercentValue;
        }
        return map;
    }
}