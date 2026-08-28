using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private GrowthManager _growthManager;
    private float _attackTimer;

    private void Update()
    {
        float attackSpeed = _growthManager.GetStat(StatType.AttackSpeed);
        if (attackSpeed <= 0f) attackSpeed = 1f; ////
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
        if (damage <= 0f) damage = 100; ////
        target.Model.ChangeCurHp(-damage);

        // 임시 로그. 나중에 제거
        Debug.Log($"[PlayerController] 공격! 데미지: {damage}, 남은 HP: {target.Model.CurHp}");

        if (target.Model.CurHp <= 0f)
        {
            _combatManager.OnMonsterKilled(target.gameObject);
        }
    }
}