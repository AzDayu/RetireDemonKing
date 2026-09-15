using TMPro;
using UnityEngine;

public class UserInformationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_RebirthGold;

    private long _lastDisplayedGold = long.MinValue;
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
        long Gold = playerModel.Gold;
        int rebirthPoints = playerModel.RebirthPoints;

        if (Text_Gold != null && _lastDisplayedGold != Gold)
        {
            _lastDisplayedGold = Gold;
            Text_Gold.text = Gold.ToString("N0");
        }

        if (Text_RebirthGold != null && _lastDisplayedRebirthPoints != rebirthPoints)
        {
            _lastDisplayedRebirthPoints = rebirthPoints;
            Text_RebirthGold.text = rebirthPoints.ToString("N0");
        }
    }
}
