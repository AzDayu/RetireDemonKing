using TMPro;
using UnityEngine;

public class UserInformationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_RebirthGold;

    private long _lastDisplayedEnhanceCurrency = long.MinValue;
    private int _lastDisplayedRebirthPoints = int.MinValue;

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Growth == null ||
            GameManager.Instance.SaveServer.GetPlayerModel() == null)
        {
            return;
        }

        PlayerModel playerModel = GameManager.Instance.SaveServer.GetPlayerModel();
        long enhanceCurrency = playerModel.EnhanceCurrency;
        int rebirthPoints = playerModel.RebirthPoints;

        if (Text_Gold != null && _lastDisplayedEnhanceCurrency != enhanceCurrency)
        {
            _lastDisplayedEnhanceCurrency = enhanceCurrency;
            Text_Gold.text = enhanceCurrency.ToString("N0");
        }

        if (Text_RebirthGold != null && _lastDisplayedRebirthPoints != rebirthPoints)
        {
            _lastDisplayedRebirthPoints = rebirthPoints;
            Text_RebirthGold.text = rebirthPoints.ToString("N0");
        }
    }
}
