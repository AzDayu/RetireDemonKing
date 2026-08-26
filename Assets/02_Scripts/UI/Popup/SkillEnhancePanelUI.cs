using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillEnhancePanelUI : MonoBehaviour
{
    private const int SkillCount = 10;
    private const long BasePromotionCost = 200;

    [Header("스킬")]
    [SerializeField] private GameObject[] Skill = new GameObject[SkillCount];

    [Header("현재 레벨 스킬 정보")]
    [SerializeField] private TMP_Text CurrentSkillName;
    [SerializeField] private TMP_Text CurrentSKillText;

    [Header("다음 레벨 스킬 정보")]
    [SerializeField] private TMP_Text NextSkillName;
    [SerializeField] private TMP_Text NextSkillText;


    [Header("버튼")]
    [SerializeField] private UIButton Button_Close;






}


