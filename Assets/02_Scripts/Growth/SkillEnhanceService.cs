using System.Collections.Generic;

public enum SkillEnhanceResult
{
    Success,
    InvalidState,
    MissingData,
    MaxLevel,
    InsufficientCurrency
}

public sealed class SkillEnhanceService
{
    private readonly PlayerSaveData _saveData;

    public SkillEnhanceService(PlayerSaveData saveData)
    {
        _saveData = saveData;

        if (_saveData != null)
        {
            _saveData.Skills ??= new List<SkillModel>();
            EnsureDefaultSkillModels();
        }
    }

    public SkillItem GetCurrentData(string skillId)
    {
        SkillModel model = FindSkillModel(skillId);
        return model == null
            ? null
            : SkillDataLoader.GetSkillData(skillId, model.Level);
    }

    public SkillItem GetNextData(string skillId)
    {
        SkillModel model = FindSkillModel(skillId);
        return model == null
            ? null
            : SkillDataLoader.GetSkillData(skillId, model.Level + 1);
    }

    public SkillEnhanceResult TryEnhance(string skillId)
    {
        if (_saveData == null || _saveData.Player == null)
        {
            return SkillEnhanceResult.InvalidState;
        }

        SkillModel model = FindSkillModel(skillId);
        if (model == null)
        {
            return SkillEnhanceResult.MissingData;
        }

        SkillItem currentData =
            SkillDataLoader.GetSkillData(skillId, model.Level);
        if (currentData == null || currentData.EnhanceCost < 0)
        {
            return SkillEnhanceResult.MissingData;
        }

        SkillItem nextData =
            SkillDataLoader.GetSkillData(skillId, model.Level + 1);
        if (nextData == null)
        {
            return SkillEnhanceResult.MaxLevel;
        }

        if (_saveData.Player.EnhanceCurrency <
            currentData.EnhanceCost)
        {
            return SkillEnhanceResult.InsufficientCurrency;
        }

        _saveData.Player.EnhanceCurrency -=
            currentData.EnhanceCost;
        model.Level = nextData.Level;

        return SkillEnhanceResult.Success;
    }

    private void EnsureDefaultSkillModels()
    {
        List<string> skillIds =
            SkillDataLoader.GetSkillIdsInDisplayOrder();

        foreach (string skillId in skillIds)
        {
            if (FindSkillModel(skillId) != null)
            {
                continue;
            }

            SkillItem firstData =
                SkillDataLoader.GetFirstSkillData(skillId);
            if (firstData == null)
            {
                continue;
            }

            _saveData.Skills.Add(
                new SkillModel
                {
                    SkillId = skillId,
                    Level = firstData.Level
                }
            );
        }
    }

    private SkillModel FindSkillModel(string skillId)
    {
        if (_saveData == null ||
            _saveData.Skills == null ||
            string.IsNullOrEmpty(skillId))
        {
            return null;
        }

        foreach (SkillModel model in _saveData.Skills)
        {
            if (model != null && model.SkillId == skillId)
            {
                return model;
            }
        }

        return null;
    }
}
