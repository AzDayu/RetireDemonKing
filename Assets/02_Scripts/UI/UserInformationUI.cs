using TMPro;
using UnityEngine;

public class UserInformationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_Gold;

    private long _lastDisplayedEnhanceCurrency =
        long.MinValue;

    private void Update()
    {
        if (Text_Gold == null ||
            GameManager.Instance == null ||
            GameManager.Instance.Growth == null ||
            GameManager.Instance.Growth.PlayerModel == null)
        {
            return;
        }

        long enhanceCurrency =
            GameManager.Instance.Growth
                .PlayerModel.EnhanceCurrency;

        if (_lastDisplayedEnhanceCurrency ==
            enhanceCurrency)
        {
            return;
        }

        _lastDisplayedEnhanceCurrency =
            enhanceCurrency;

        Text_Gold.text =
            enhanceCurrency.ToString("X");
    }
}