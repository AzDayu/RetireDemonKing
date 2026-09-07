using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("=== 컴포넌트 참조 ===")]
    [SerializeField] private PlayerAnimationView _animationView;

    [Header("=== 무기 외형 슬롯 ===")]
    [SerializeField] private List<WeaponSlot> _weaponSlots = new List<WeaponSlot>();
    [SerializeField] private WeaponType _currentWeapon = WeaponType.Sword;

    [Header("=== 전투 설정 ===")]
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private LayerMask _monsterLayer;

    public float CurHp { get; private set; }
    public float MaxHp { get; private set; }
    public bool IsDead => CurHp <= 0f;
    public event System.Action<float, float> OnHpChanged;

    private readonly Collider[] _detectResults = new Collider[1];
    private GrowthManager _subscribedGrowthManager;

    private void Awake()
    {
        Instance = this;
        if (_animationView == null)
            _animationView = GetComponent<PlayerAnimationView>();
    }

    public void Initialize()
    {
        ChangeWeapon(_currentWeapon);
        SubscribeToGrowthStats();
        ResetHp();
        Debug.Log($"[PlayerController] 초기화 완료. MaxHp: {MaxHp}");
    }

    public void ResetHp()
    {
        MaxHp = GameManager.Instance != null && GameManager.Instance.Growth != null
            ? GameManager.Instance.Growth.GetStatValue(StatType.MaxHp)
            : 100f;

        CurHp = MaxHp;
        NotifyHpChanged();
    }

    public void ChangeWeapon(WeaponType newWeapon)
    {
        _currentWeapon = newWeapon;
        foreach (var slot in _weaponSlots)
        {
            if (slot.WeaponObject != null)
                slot.WeaponObject.SetActive(slot.WeaponType == newWeapon);
        }
        _animationView?.SetWeaponType(_currentWeapon);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.IdleStage)
            return;

        if (IsDead) return;
        HandleCombatLogic();
    }

    private void OnEnable()
    {
        SubscribeToGrowthStats();

        if (_animationView != null)
        {
            _animationView.OnAttackHit += HandleAttackHit;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromGrowthStats();

        if (_animationView != null)
        {
            _animationView.OnAttackHit -= HandleAttackHit;
        }
    }

    private void SubscribeToGrowthStats()
    {
        GrowthManager growthManager = GameManager.Instance?.Growth;
        if (_subscribedGrowthManager == growthManager)
        {
            return;
        }

        UnsubscribeFromGrowthStats();
        _subscribedGrowthManager = growthManager;

        if (_subscribedGrowthManager != null)
        {
            _subscribedGrowthManager.OnStatsUpdated += HandleStatsUpdated;
        }
    }

    private void UnsubscribeFromGrowthStats()
    {
        if (_subscribedGrowthManager != null)
        {
            _subscribedGrowthManager.OnStatsUpdated -= HandleStatsUpdated;
            _subscribedGrowthManager = null;
        }
    }

    private void HandleStatsUpdated()
    {
        if (_subscribedGrowthManager == null)
        {
            return;
        }

        float currentHpRatio = MaxHp > 0f
            ? Mathf.Clamp01(CurHp / MaxHp)
            : 1f;

        MaxHp = Mathf.Max(
            0f,
            _subscribedGrowthManager.GetStat(StatType.MaxHp)
        );
        CurHp = MaxHp * currentHpRatio;
        NotifyHpChanged();
    }

    private void NotifyHpChanged()
    {
        OnHpChanged?.Invoke(CurHp, MaxHp);
    }

    private void HandleCombatLogic()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _attackRange, _detectResults, _monsterLayer);
        bool hasTarget = hitCount > 0;

        _animationView?.PlayAttack(hasTarget);
        _animationView?.PlayMove(!hasTarget);

        if (hasTarget)
        {
            float attackSpeed = GameManager.Instance.Growth.GetStatValue(StatType.AttackSpeed);
            attackSpeed = attackSpeed > 0f ? attackSpeed : 1f;
            _animationView?.SetAnimationSpeed(attackSpeed);
        }
        else
        {
            _animationView?.SetAnimationSpeed(1f);
        }
    }

    private void HandleAttackHit()
    {
        if (IsDead) return;

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _attackRange, _detectResults, _monsterLayer);
        if (hitCount > 0)
        {
            ExecuteAttack(_detectResults[0]);
        }
    }

    private void ExecuteAttack(Collider targetCollider)
    {
        if (targetCollider == null) return;

        MonsterController monster = targetCollider.GetComponent<MonsterController>()
                                 ?? targetCollider.GetComponentInParent<MonsterController>();

        if (monster == null || monster.IsDead) return;

        float damage = GameManager.Instance.Growth.GetStatValue(StatType.Attack);
        float accuracy = GameManager.Instance.Growth.GetStatValue(StatType.Accuracy);

        Debug.Log($"[Player 공격 성공] 대상: {monster.name} | 피해량: {damage} | 명중률: {accuracy}");

        monster.TakeDamage(damage, accuracy);
    }

    public void TakeDamage(float monsterAttackPower, float monsterAccuracy = 100f)
    {
        if (IsDead) return;

        float evasion = GameManager.Instance.Growth.GetStatValue(StatType.Evasion);
        float hitChance = Mathf.Clamp(monsterAccuracy - evasion, 10f, 100f);
        if (UnityEngine.Random.Range(0f, 100f) > hitChance) return;

        float defense = GameManager.Instance.Growth.GetStatValue(StatType.Defense);
        float finalDamage = Mathf.Max(1f, monsterAttackPower - defense);

        CurHp = Mathf.Max(0f, CurHp - finalDamage);
        NotifyHpChanged();

        if (CurHp <= 0f)
        {
            _animationView?.PlayDie();
            GameManager.Instance.Combat?.TriggerBattleFailed();
        }
    }
}
