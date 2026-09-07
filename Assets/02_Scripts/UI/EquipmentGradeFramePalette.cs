using UnityEngine;

/// <summary>
/// 장비 등급별 슬롯 프레임 프리팹을 한 곳에서 관리한다.
/// Resources의 EquipmentGradeFramePalette 에셋을 모든 장비 UI가 공유한다.
/// </summary>
[CreateAssetMenu(
    fileName = "EquipmentGradeFramePalette",
    menuName = "Game/Equipment Grade Frame Palette"
)]
public sealed class EquipmentGradeFramePalette : ScriptableObject
{
    [SerializeField] private RectTransform _commonFrame;
    [SerializeField] private RectTransform _rareFrame;
    [SerializeField] private RectTransform _epicFrame;
    [SerializeField] private RectTransform _legendaryFrame;
    [SerializeField] private RectTransform _mythicFrame;

    public RectTransform GetFramePrefab(EquipmentGrade grade)
    {
        return grade switch
        {
            EquipmentGrade.Rare => _rareFrame,
            EquipmentGrade.Epic => _epicFrame,
            EquipmentGrade.Legendary => _legendaryFrame,
            EquipmentGrade.Mythic => _mythicFrame,
            _ => _commonFrame
        };
    }
}
