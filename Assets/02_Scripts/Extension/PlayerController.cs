using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Transform Instance { get; private set; }

    [Header("=== 컴포넌트 참조 ===")]
    [SerializeField] private PlayerAnimationView _animationView;

    [Header("=== 무기 슬롯 설정 ===")]
    [SerializeField] private List<WeaponSlot> _weaponSlots = new List<WeaponSlot>();
    [SerializeField] private WeaponType _currentWeapon = WeaponType.Sword;

    [Header("=== 전투 설정 ===")]
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private LayerMask _monsterLayer;

    private WeaponHitbox _currentWeaponHitbox;
    private readonly Collider[] _detectResults = new Collider[5];

    private void Awake()
    {
        Instance = transform;

        if (_animationView == null)
        {
            _animationView = GetComponent<PlayerAnimationView>();
        }
    }

    private void OnEnable()
    {
        if (_animationView != null)
        {
            _animationView.OnSwingStart += EnableCurrentWeaponCollider;
            _animationView.OnSwingEnd += DisableCurrentWeaponCollider;
        }
    }

    private void OnDisable()
    {
        if (_animationView != null)
        {
            _animationView.OnSwingStart -= EnableCurrentWeaponCollider;
            _animationView.OnSwingEnd -= DisableCurrentWeaponCollider;
        }
    }

    private void Start()
    {
        ChangeWeapon(_currentWeapon);  
    }

    private void Update()
    {
        HandleCombatLogic();  
    }

    public void ChangeWeapon(WeaponType newWeapon)
    {
        _currentWeapon = newWeapon;  
        _currentWeaponHitbox = null;

        foreach (var slot in _weaponSlots)
        {
            bool isCurrent = slot.WeaponType == newWeapon;

            if (slot.WeaponObject != null)
            {
                slot.WeaponObject.SetActive(isCurrent);
            }

            if (isCurrent)
            {
                _currentWeaponHitbox = slot.WeaponHitbox;
                _currentWeaponHitbox?.DisableHitbox();
            }
        }

        if (_animationView != null)  
        {
            _animationView.SetWeaponType(_currentWeapon);  
        }
    }

    private void HandleCombatLogic()
    {
        if (GameManager.Instance == null || GameManager.Instance.Growth == null) return;  

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _attackRange, _detectResults, _monsterLayer);  
        bool hasTarget = hitCount > 0;  

        if (_animationView != null)  
        {
            _animationView.PlayAttack(hasTarget);  
            _animationView.PlayMove(!hasTarget);  

            if (hasTarget)  
            {
                float attackSpeed = GameManager.Instance.Growth.GetStat(StatType.AttackSpeed);  
                _animationView.SetAnimationSpeed(attackSpeed);  
            }
            else
            {
                DisableCurrentWeaponCollider();
            }
        }
    }

    private void EnableCurrentWeaponCollider()
    {
        _currentWeaponHitbox?.EnableHitbox();
    }

    private void DisableCurrentWeaponCollider()
    {
        _currentWeaponHitbox?.DisableHitbox();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}