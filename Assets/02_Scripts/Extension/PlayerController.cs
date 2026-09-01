using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== 애니메이터 참조 ===")]
    [SerializeField] private Animator _animator;

    private float _attackTimer;

    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        // 예시: 공격 속도 스탯이 필요할 때는 GameManager를 통해 조회
        float attackSpeed = GameManager.Instance.Growth.GetStat(StatType.AttackSpeed);

        // 공격 쿨타임 및 공격 모션 제어 로직...
    }

    public void PlayAttackAnimation()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(HashAttack);
        }
    }

    // 애니메이션 타격 프레임 이벤트
    public void OnAttackHitEvent()
    {
        // 타격 판정은 CombatManager에 위임
        // GameManager.Instance.Combat.OnPlayerHitTarget();
    }
}