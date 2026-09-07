using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class EquipmentSlotIconUI
{
    private const string EmptyIconName = "Icon_Empty";
    private const string EmptyFrameName = "ItemFrame_05_Empty";
    private const string ItemIconName = "Item";
    private const string LevelTextPrefix = "Text_Level";
    private const string GradeFramePrefix = "EquipmentGradeFrame_";
    private const string GradeFramePalettePath = "EquipmentGradeFramePalette";

    private static EquipmentGradeFramePalette _gradeFramePalette;

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

        SetEmptyFrameActive(slotRoot, true);
        SetGradeFramesInactive(slotRoot);

        Transform emptyIcon = FindNamedChild(slotRoot, EmptyIconName);
        Transform itemIcon = FindNamedChild(slotRoot, ItemIconName);
        TMP_Text levelText = FindLevelText(slotRoot);

        if (emptyIcon != null) emptyIcon.gameObject.SetActive(true);
        if (itemIcon != null) itemIcon.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
    }

    public static bool ShowEquipment(
        Transform slotRoot,
        Sprite sprite,
        int level,
        EquipmentGrade grade)
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

        if (!ShowGradeFrame(slotRoot, grade))
            SetEmptyFrameActive(slotRoot, true);

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPaletteCache()
    {
        _gradeFramePalette = null;
    }

    private static bool ShowGradeFrame(Transform slotRoot, EquipmentGrade grade)
    {
        EquipmentGradeFramePalette palette = GetGradeFramePalette();
        RectTransform framePrefab = palette != null
            ? palette.GetFramePrefab(grade)
            : null;

        if (framePrefab == null)
        {
            Debug.LogWarning($"[장비 UI] {grade} 등급 프레임 프리팹을 찾지 못했습니다.");
            return false;
        }

        string frameName = GradeFramePrefix + grade;
        Transform targetFrame = null;

        foreach (Transform child in slotRoot)
        {
            if (!child.name.StartsWith(GradeFramePrefix)) continue;

            bool isTarget = child.name == frameName;
            child.gameObject.SetActive(isTarget);
            if (isTarget) targetFrame = child;
        }

        if (targetFrame == null)
        {
            RectTransform frameRect;

            try
            {
                frameRect = Object.Instantiate(framePrefab, slotRoot, false);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"[장비 UI] {grade} 등급 프레임 생성 실패: {exception.Message}"
                );
                return false;
            }

            GameObject frameObject = frameRect.gameObject;
            frameObject.name = frameName;
            targetFrame = frameRect;

            foreach (Graphic graphic in frameObject.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            targetFrame.SetSiblingIndex(0);
        }

        if (targetFrame is RectTransform targetFrameRect)
            MatchEmptyFrameBounds(slotRoot, targetFrameRect);

        SetEmptyFrameActive(slotRoot, false);
        targetFrame.gameObject.SetActive(true);
        return true;
    }

    private static void MatchEmptyFrameBounds(
        Transform slotRoot,
        RectTransform gradeFrame)
    {
        RectTransform emptyFrame = FindNamedChild(slotRoot, EmptyFrameName)
            as RectTransform;

        if (emptyFrame == null)
        {
            gradeFrame.anchorMin = Vector2.zero;
            gradeFrame.anchorMax = Vector2.one;
            gradeFrame.offsetMin = Vector2.zero;
            gradeFrame.offsetMax = Vector2.zero;
            gradeFrame.localScale = Vector3.one;
            return;
        }

        bool wasActive = emptyFrame.gameObject.activeSelf;
        if (!wasActive) emptyFrame.gameObject.SetActive(true);

        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            slotRoot,
            emptyFrame
        );

        gradeFrame.anchorMin = new Vector2(0.5f, 0.5f);
        gradeFrame.anchorMax = new Vector2(0.5f, 0.5f);
        gradeFrame.pivot = new Vector2(0.5f, 0.5f);
        gradeFrame.anchoredPosition = new Vector2(bounds.center.x, bounds.center.y);
        gradeFrame.sizeDelta = new Vector2(bounds.size.x, bounds.size.y);
        gradeFrame.localScale = Vector3.one;

        if (!wasActive) emptyFrame.gameObject.SetActive(false);
    }

    private static EquipmentGradeFramePalette GetGradeFramePalette()
    {
        if (_gradeFramePalette == null)
        {
            _gradeFramePalette = Resources.Load<EquipmentGradeFramePalette>(
                GradeFramePalettePath
            );
        }

        return _gradeFramePalette;
    }

    private static void SetEmptyFrameActive(Transform slotRoot, bool isActive)
    {
        Transform emptyFrame = FindNamedChild(slotRoot, EmptyFrameName);
        if (emptyFrame != null) emptyFrame.gameObject.SetActive(isActive);
    }

    private static void SetGradeFramesInactive(Transform slotRoot)
    {
        foreach (Transform child in slotRoot)
        {
            if (child.name.StartsWith(GradeFramePrefix))
            {
                child.gameObject.SetActive(false);
            }
        }
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
