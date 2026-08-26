using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillEnhancePanelUI : MonoBehaviour
{
    private const long BasePromotionCost = 200;

    [Header("스킬")]
    [SerializeField] private UIButton[] skillButtons;

    private int _selectedSkillIndex;

    [Header("현재 레벨 스킬 정보")]
    [SerializeField] private TMP_Text CurrentSkillName;
    [SerializeField] private TMP_Text CurrentSkillText;

    [Header("다음 레벨 스킬 정보")]
    [SerializeField] private TMP_Text NextSkillName;
    [SerializeField] private TMP_Text NextSkillText;


    [Header("버튼")]
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_Enhance;


    [Serializable]
    private class TempSkillData
    {
        public string Name;

        [TextArea]
        public string CurrentDescription;

        public string NextName;

        [TextArea]
        public string NextDescription;
    }

    PlayerModel playerModel = GameManager.Instance?.Growth?.PlayerModel;

    [Header("임시 스킬 데이터")]
    [SerializeField] private TempSkillData[] tempSkillDataList;

    private void OnEnable()
    {
        if (skillButtons.Length != tempSkillDataList.Length)
        {
            Debug.LogWarning(
                $"스킬 버튼 {skillButtons.Length}개, " +
                $"임시 데이터 {tempSkillDataList.Length}개. " +
                $"개수 맞춰야 함."
            );
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            int index = i;

            skillButtons[i]?.BindOnClickButtonEvent(
                () => SelectSkill(index),
                true
            );
        }

        if (tempSkillDataList.Length > 0)
        {
            SelectSkill(0);
        }


        Button_Enhance?.BindOnClickButtonEvent(
            OnClickEnhance,
            true
        );

        Button_Close?.BindOnClickButtonEvent(
            OnClickClose,
            true
        );
    }

    private void OnDisable()
    {
        foreach (UIButton button in skillButtons)
        {
            button?.UnBindAllOnClickButtonEvent();
        }

        Button_Enhance?.UnBindAllOnClickButtonEvent();
        Button_Close?.UnBindAllOnClickButtonEvent();
    }

    private void SelectSkill(int index)
    {
        if (tempSkillDataList == null ||
            index < 0 ||
            index >= tempSkillDataList.Length)
        {
            Debug.LogWarning($"[스킬 강화] {index}번 스킬 데이터 없음.");
            return;
        }

        _selectedSkillIndex = index;
        RefreshSelectedSkillUI();
    }

    private void RefreshSelectedSkillUI()
    {
        TempSkillData skill = tempSkillDataList[_selectedSkillIndex];

        CurrentSkillName.text = skill.Name;
        CurrentSkillText.text = skill.CurrentDescription;

        bool hasNextLevel = string.IsNullOrWhiteSpace(skill.NextName) == false;

        if (hasNextLevel)
        {
            NextSkillName.text = skill.NextName;
            NextSkillText.text = skill.NextDescription;
        }
        else
        {
            NextSkillName.text = "MAX";
            NextSkillText.text = "최고 레벨입니다.";
        }

        if (Button_Enhance != null)
        {
            Button_Enhance.gameObject.SetActive(true);
        }
    }

    private void OnClickEnhance()
    {
        if (tempSkillDataList == null ||
            _selectedSkillIndex < 0 ||
            _selectedSkillIndex >= tempSkillDataList.Length)
        {
            return;
        }

        PlayerModel playerModel =
            GameManager.Instance?.Growth?.PlayerModel;

        if (playerModel == null)
        {
            Debug.LogWarning("[스킬 강화] 플레이어 데이터가 없습니다.");
            return;
        }

        TempSkillData skill =
            tempSkillDataList[_selectedSkillIndex];

        if (string.IsNullOrWhiteSpace(skill.NextName))
        {
            Debug.Log("[스킬 강화] 이미 최고 레벨입니다.");
            return;
        }

        if (playerModel.EnhanceCurrency < BasePromotionCost)
        {
            Debug.Log("[스킬 강화] 강화 재화가 부족합니다.");
            return;
        }

        playerModel.EnhanceCurrency -= BasePromotionCost;

        skill.Name = skill.NextName;
        skill.CurrentDescription = skill.NextDescription;

        skill.NextName = string.Empty;
        skill.NextDescription = string.Empty;

        RefreshSelectedSkillUI();
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
    }

}



