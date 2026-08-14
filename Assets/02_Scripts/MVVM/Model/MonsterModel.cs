using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterModel
{
    public event Action<string> OnInfoChanged;

    private string _monsterId;
    private string _monsterName;
    private float _maxHp;
    private float _curHp;
    private float _attackPower;
    private float _attackSpeed;
    private int _dropCoins;
    private float _dropExp;
    private List<DropItemData> _dropTable;

    public string MonsterId
    {
        get { return _monsterId; }
        set { _monsterId = value; OnInfoChanged?.Invoke(nameof(MonsterId)); }
    }
    public string MonsterName
    {
        get { return _monsterName; }
        set { _monsterName = value; OnInfoChanged?.Invoke(nameof(MonsterName)); }
    }
    public int DropCoins
    {
        get { return _dropCoins; }
        set 
        { 
            _dropCoins = value; 
            OnInfoChanged?.Invoke(nameof(DropCoins)); 
        }
    }
    public float DropExp
    {
        get { return _dropExp; }
        set { _dropExp = value; OnInfoChanged?.Invoke(nameof(DropExp)); }
    }
    public List<DropItemData> DropTable
    {
        get { return _dropTable; }
        set 
        {
            _dropTable = value != null
            ? new List<DropItemData>(value)
            : new List<DropItemData>();
            OnInfoChanged?.Invoke(nameof(DropTable)); 
        }
    }
    public float MaxHp
    {
        get { return _maxHp; }
        set
        {
            _maxHp = Mathf.Max(0f, value);
            if (_curHp > _maxHp)
            {
                CurHp = _maxHp;
            }
            OnInfoChanged?.Invoke(nameof(MaxHp));
        }
    }
    public float CurHp
    {
        get { return _curHp; }
        set
        {
            _curHp = Mathf.Clamp(value, 0f, _maxHp);
            OnInfoChanged?.Invoke(nameof(CurHp));
        }
    }
    public float AttackPower
    {
        get { return _attackPower; }
        set { _attackPower = value; OnInfoChanged?.Invoke(nameof(AttackPower)); }
    }
    public float AttackSpeed
    {
        get { return _attackSpeed; }
        set { _attackSpeed = value; OnInfoChanged?.Invoke(nameof(AttackSpeed)); }
    }


    public MonsterModel(MonsterData data)
    {
        if (data == null)
        {
            _dropTable = new List<DropItemData>();
            return;
        }

        _monsterId = data.MonsterId;
        _monsterName = data.MonsterName;
        _maxHp = data.MaxHp;
        _curHp = data.MaxHp;
        _attackPower = data.AttackPower;
        _attackSpeed = data.AttackSpeed;
        _dropCoins = data.DropCoins;
        _dropExp = data.DropExp;
        _dropTable = data.DropTable != null
            ? new List<DropItemData>(data.DropTable) 
            : new List<DropItemData>();
    }

    public void ChangeCurHp(float amount)
    {
        CurHp += amount;

        if (CurHp > MaxHp)
        {
            CurHp = MaxHp;
        }
        if (CurHp < 0f)
        {
            CurHp = 0f;
        }
    }
}