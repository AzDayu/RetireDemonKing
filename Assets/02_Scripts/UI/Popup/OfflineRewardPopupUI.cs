using TMPro;
using UnityEngine;

public class OfflineRewardPopupUI : UIBase
{
    [SerializeField] private TMP_Text Text_OfflineTime;
    [SerializeField] private TMP_Text Text_Gold;
    [SerializeField] private TMP_Text Text_Experience;
    [SerializeField] private TMP_Text Text_Notice;
    [SerializeField] private UIButton Button_Confirm;

    public void SetUI(OfflineRewardResult reward)
    {
        if (reward == null)
        {
            return;
        }

        long hours = reward.ElapsedMinutes / 60;
        long minutes = reward.ElapsedMinutes % 60;

        if (Text_OfflineTime != null)
        {
            Text_OfflineTime.text = $"보상 인정 시간: {hours}시간 {minutes}분";
        }

        if (Text_Gold != null)
        {
            Text_Gold.text = $"골드 +{reward.Gold:N0}";
        }

        if (Text_Experience != null)
        {
            Text_Experience.text = $"경험치 +{reward.Experience:N0}";
        }

        if (Text_Notice != null)
        {
            Text_Notice.text = reward.WasTimeCapped
                ? "최대 인정 시간까지만 계산했습니다.\n보상이 지급되었습니다."
                : "보상이 지급되었습니다.";
        }

        BindButtons();
        transform.SetAsLastSibling();
    }

    private void BindButtons()
    {
        if (Button_Confirm == null)
        {
            Debug.LogError("[OfflineRewardPopupUI] 확인 버튼 참조가 비어 있습니다.");
            return;
        }

        Button_Confirm.UnBindAllOnClickButtonEvent();
        Button_Confirm.BindOnClickButtonEvent(OnClickConfirm);
    }

    private void OnClickConfirm()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.UI == null)
        {
            return;
        }

        GameManager.Instance.UI.CloseOfflineRewardPopupUI();
    }
}
