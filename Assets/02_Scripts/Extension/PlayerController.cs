using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private GrowthManager _growthManager;
    private float _attackTimer;

    private void Update()
    {
        float attackSpeed = _growthManager.GetStat(StatType.AttackSpeed);
        float attackInterval = 1f / Mathf.Max(0.01f, attackSpeed);

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= attackInterval)
        {
            _attackTimer -= attackInterval;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        MonsterController target = _combatManager.GetActiveMonster();
        if (target == null || target.Model == null) return;

        float damage = _growthManager.GetStat(StatType.Attack);
        target.Model.ChangeCurHp(-damage);

        if (target.Model.CurHp <= 0f)
        {
            _combatManager.OnMonsterKilled(target.gameObject);
        }
    }
}