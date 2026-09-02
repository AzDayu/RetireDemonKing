using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [SerializeField] private Collider _weaponCollider;

    private readonly HashSet<MonsterController> _hitTargets = new HashSet<MonsterController>();

    private void Awake()
    {
        if (_weaponCollider == null)
        {
            _weaponCollider = GetComponent<Collider>();
        }

        DisableHitbox();
    }

    public void EnableHitbox()
    {
        _hitTargets.Clear();
        if (_weaponCollider != null)
        {
            _weaponCollider.enabled = true;
        }
    }

    public void DisableHitbox()
    {
        if (_weaponCollider != null)
        {
            _weaponCollider.enabled = false;
        }
        _hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        MonsterController target = other.GetComponent<MonsterController>();
        if (target == null || target.Model == null || target.Model.CurHp <= 0f) return;

        if (_hitTargets.Contains(target)) return;
        _hitTargets.Add(target);

        float damage = GameManager.Instance.Growth.GetStat(StatType.Attack);
        target.Model.ChangeCurHp(-damage);

        Debug.Log($"[WeaponHitbox] 타격! 피해량: {damage}, 남은 HP: {target.Model.CurHp}");

        if (target.Model.CurHp <= 0f)
        {
            GameManager.Instance.Combat.OnMonsterKilled(target.gameObject);
        }
    }
}