using TMPro;
using UnityEngine;

public class StageTextUI : UIBase
{
    [SerializeField] private TextMeshProUGUI _stage_Text;

    private StageManager _stageManager;

    private void Awake()
    {
        InitStageManager();
    }

    private void InitStageManager()
    {
        _stageManager = GameManager.Instance.Stage;
    }

    private void OnEnable()
    {
        UpdateStageText(_stageManager.CurrentStage);
        _stageManager.OnStageChanged += UpdateStageText;
    }

    private void OnDisable()
    {
        _stageManager.OnStageChanged -= UpdateStageText;
    }

    private void UpdateStageText(int stage)
    {
        _stage_Text.text = $"Stage - {stage}";
    }
}