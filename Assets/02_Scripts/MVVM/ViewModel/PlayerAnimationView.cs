using System;
using UnityEngine;

public class PlayerAnimationView : CharacterAnimationView
{
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");

    public event Action OnSwingStart;
    public event Action OnSwingEnd;

    public void SetWeaponType(WeaponType weaponType)
    {
        if (_animator != null && _validParameterHashSet.Contains(HashWeaponType))
        {
            _animator.SetInteger(HashWeaponType, (int)weaponType);
        }
    }

    public override void PlayAttack(bool isAttacking)
    {
        base.PlayAttack(isAttacking);

        if (!isAttacking)
        {
            OnSwingEnd?.Invoke();
        }
    }

    public override void PlayDie()
    {
        OnSwingEnd?.Invoke();
        base.PlayDie();
    }

    public void OnAttackSwingStartEvent()
    {
        OnSwingStart?.Invoke();
    }

    public void OnAttackSwingEndEvent()
    {
        OnSwingEnd?.Invoke();
    }
}
