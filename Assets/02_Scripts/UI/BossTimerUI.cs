using UnityEngine;
using TMPro;

public class BossTimerUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Timer;

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBossTimerUpdated += HandleBossTimerUpdated;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Combat != null)
        {
            GameManager.Instance.Combat.OnBossTimerUpdated -= HandleBossTimerUpdated;
        }
    }

    private void HandleBossTimerUpdated(float currentTime, float maxTime)
    {
        if (Text_Timer == null) return;

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(currentTime, 0f));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        Text_Timer.text = $"{minutes:00}:{seconds:00}";
    }
}