using UnityEngine;

public class PlayerAnimationView : CharacterAnimationView
{
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");

    public void SetWeaponType(WeaponType weaponType)
    {
        if (_animator != null && _validParameterHashSet.Contains(HashWeaponType))
        {
            _animator.SetInteger(HashWeaponType, (int)weaponType);
        }
    }
}
