using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationView : MonoBehaviour
{
    [Header("=== 애니메이터 참조 ===")]
    [SerializeField] protected Animator _animator;

    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashDie = Animator.StringToHash("Die");

    protected readonly HashSet<int> _validParameterHashSet = new HashSet<int>();

    protected virtual void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        CacheValidParameters();
    }

    protected void CacheValidParameters()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        _validParameterHashSet.Clear();
        foreach (var param in _animator.parameters)
        {
            _validParameterHashSet.Add(param.nameHash);
        }
    }

    public virtual void PlayMove(bool isMoving)
    {
        if (_animator != null && _validParameterHashSet.Contains(HashIsMoving))
        {
            _animator.SetBool(HashIsMoving, isMoving);
        }
    }

    public virtual void PlayAttack(bool isAttacking)
    {
        if (_animator != null && _validParameterHashSet.Contains(HashIsAttacking))
        {
            _animator.SetBool(HashIsAttacking, isAttacking);
        }
    }

    public virtual void SetAnimationSpeed(float speedMultiplier)
    {
        if (_animator != null)
        {
            _animator.speed = speedMultiplier;
        }
    }

    public virtual void PlayDie()
    {
        if (_animator != null)
        {
            PlayAttack(false);
            PlayMove(false);

            if (_validParameterHashSet.Contains(HashDie))
            {
                _animator.SetTrigger(HashDie);
            }
        }
    }
}
