using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 기존 장비 슬롯 프리팹의 빈 아이콘, 장비 아이콘, 레벨 표시를 갱신한다.
/// 슬롯 프리팹을 다시 제작하지 않고 현재 계층 이름을 공통 규칙으로 사용한다.
/// </summary>
public static class EquipmentSlotIconUI
{
    private const string EmptyIconName = "Icon_Empty";
    private const string ItemIconName = "Item";
    private const string LevelTextPrefix = "Text_Level";

    public static Transform FindSlot(Transform root, string slotName)
    {
        if (root == null || string.IsNullOrEmpty(slotName)) return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == slotName) return child;
        }

        return null;
    }

    public static void ShowEmpty(Transform slotRoot)
    {
        if (slotRoot == null) return;

        Transform emptyIcon = FindNamedChild(slotRoot, EmptyIconName);
        Transform itemIcon = FindNamedChild(slotRoot, ItemIconName);
        TMP_Text levelText = FindLevelText(slotRoot);

        if (emptyIcon != null) emptyIcon.gameObject.SetActive(true);
        if (itemIcon != null) itemIcon.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
    }

    public static bool ShowEquipment(Transform slotRoot, Sprite sprite, int level)
    {
        if (slotRoot == null || sprite == null)
        {
            ShowEmpty(slotRoot);
            return false;
        }

        Transform itemIconTransform = FindNamedChild(slotRoot, ItemIconName);
        Image itemIcon = itemIconTransform != null
            ? itemIconTransform.GetComponent<Image>()
            : null;

        if (itemIcon == null)
        {
            Debug.LogWarning($"[장비 UI] 장비 아이콘 Image를 찾지 못했습니다: {slotRoot.name}");
            ShowEmpty(slotRoot);
            return false;
        }

        Transform emptyIcon = FindNamedChild(slotRoot, EmptyIconName);
        TMP_Text levelText = FindLevelText(slotRoot);

        itemIcon.sprite = sprite;
        itemIcon.color = Color.white;
        itemIcon.preserveAspect = true;
        itemIcon.gameObject.SetActive(true);

        if (emptyIcon != null) emptyIcon.gameObject.SetActive(false);
        if (levelText != null)
        {
            levelText.text = $"Lv.{Mathf.Max(1, level)}";
            levelText.gameObject.SetActive(true);
        }

        return true;
    }

    private static Transform FindNamedChild(Transform slotRoot, string objectName)
    {
        foreach (Transform child in slotRoot)
        {
            if (child.name == objectName) return child;
        }

        foreach (Transform child in slotRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child != slotRoot && child.name == objectName) return child;
        }

        return null;
    }

    private static TMP_Text FindLevelText(Transform slotRoot)
    {
        foreach (TMP_Text text in slotRoot.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name.StartsWith(LevelTextPrefix)) return text;
        }

        return null;
    }
}
