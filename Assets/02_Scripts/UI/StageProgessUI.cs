using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class StageProgressUI : UIBase
{
    [SerializeField] private Image Image_Fill;
    [SerializeField] private RectTransform Rect_Point;
    [SerializeField] private RectTransform Rect_Bar;

    private void Awake()
    {
        InitStageProgressUI();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Stage != null)
        {
            GameManager.Instance.Stage.OnStageChanged += HandleStageChanged;
            UpdateProgress();
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.Stage != null)
        {
            GameManager.Instance.Stage.OnStageChanged -= HandleStageChanged;
        }
    }

    private void InitStageProgressUI()
    {
        if (Rect_Bar == null)
        {
            Rect_Bar = transform as RectTransform;
        }
    }

    private void HandleStageChanged(int stage)
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        var stage = GameManager.Instance.Stage;
        int stageInTheme = (stage.CurrentStage - 1) % stage.StagesForChange + 1;
        float progress = (float)stageInTheme / stage.StagesForChange;

        if (Image_Fill != null)
        {
            Image_Fill.fillAmount = progress;
        }

        if (Rect_Point != null && Rect_Bar != null)
        {
            float width = Rect_Bar.rect.width;
            Vector2 pos = Rect_Point.anchoredPosition;
            pos.x = width * progress;
            Rect_Point.anchoredPosition = pos;
        }
    }
}