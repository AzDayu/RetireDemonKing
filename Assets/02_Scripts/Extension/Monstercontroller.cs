using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("=== 애니메이터 참조 ===")]
    [SerializeField] private Animator _animator;

    public MonsterModel Model { get; private set; }
    public void Setup(MonsterData data) { }
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    public void PlayAttackAnimation()
    {
        if (_animator != null) _animator.SetTrigger(HashAttack);
    }

    public void PlayHitAnimation()
    {
        if (_animator != null) _animator.SetTrigger(HashHit);
    }

    public void PlayDieAnimation()
    {
        if (_animator != null) _animator.SetTrigger(HashDie);
    }
}