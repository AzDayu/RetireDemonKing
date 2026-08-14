using System;
using UnityEngine;

public class PlayerModelV2
{
    private PlayerStats _playerStats;
    public PlayerStats Stats => _playerStats;

    public event Action<string> OnInfoChanged;

    private string _name;
    private int _curLevel = 1;
    private float _totalExp;
    private int _rebirthPoints;
    private int _coins;
    private int _curStage;
    private float _curHp;
    
    public string Name
    {
        get => _name;
        set { _name = value; OnInfoChanged?.Invoke(nameof(Name)); }
    }
    public int CurLevel
    {
        get => _curLevel;
        set
        {
            int nextLevel = Mathf.Max(1, value);

            if (_curLevel == nextLevel)
            {
                return;
            }

            _curLevel = nextLevel;
            OnInfoChanged?.Invoke(nameof(CurLevel));
        }
    }
    public float TotalExp
    {
        get => _totalExp;
        set
        {
            _totalExp = value;

            OnInfoChanged?.Invoke(nameof(TotalExp));
        }
    }
    public int RebirthPoints
    {
        get => _rebirthPoints;
        set { _rebirthPoints = value; OnInfoChanged?.Invoke(nameof(RebirthPoints)); }
    }
    public int Coins
    {
        get => _coins;
        set { _coins = value; OnInfoChanged?.Invoke(nameof(Coins)); }
    }
    public int CurStage
    {
        get => _curStage;
        set { _curStage = value; OnInfoChanged?.Invoke(nameof(CurStage)); }
    }
    public float CurHp
    {
        get => _curHp;
        set { _curHp = value; OnInfoChanged?.Invoke(nameof(CurHp)); }
    }


    public PlayerModelV2(PlayerStats playerStats)
    {
        _playerStats = playerStats;
        _playerStats.OnStatUpdated += HandleStatUpdated;
    }

    private void HandleStatUpdated(StatType type)
    {
        if (type == StatType.BaseMaxHP)
        {
            float newMaxHp = _playerStats.GetValue(StatType.BaseMaxHP);
            if (CurHp > newMaxHp) CurHp = newMaxHp;
        }
    }

    public void Dispose()
    {
        _playerStats.OnStatUpdated -= HandleStatUpdated;
    }

    public void AddExperience(float experience)
    {
        if (experience <= 0f)
        {
            return;
        }

        TotalExp += experience;
    }

    public void AddCoins(int coins)
    {
        if (coins <= 0)
        {
            return;
        }

        Coins += coins;
    }

    public void AddRebirthPoints(int rebirthPoints)
    {
        if (rebirthPoints <= 0)
        {
            return;
        }

        RebirthPoints += rebirthPoints;
    }

    public void ChangeCurHp(float amount)
    {
        CurHp += amount;
        float maxHp = _playerStats.GetValue(StatType.BaseMaxHP);

        if (CurHp > maxHp)
        {
            CurHp = maxHp;
        }
        if (CurHp < 0f)
        {
            CurHp = 0f;
        }
    }
}
