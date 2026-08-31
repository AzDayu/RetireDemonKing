using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CombatManager _combatManager;
    [SerializeField] private GrowthManager _growthManager;
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private LayerMask _monsterLayer;
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
        Collider[] hits = Physics.OverlapSphere(transform.position, _attackRange, _monsterLayer);
        float damage = _growthManager.GetStat(StatType.Attack);
        if (damage <= 0f) damage = 50; ////

        foreach (var hit in hits)
        {
            MonsterController target = hit.GetComponent<MonsterController>();
            if (target == null || target.Model == null) continue;

            target.Model.ChangeCurHp(-damage);
            // 임시 로그. 나중에 제거
            Debug.Log($"[PlayerController] 공격! 데미지: {damage}, 남은 HP: {target.Model.CurHp}");

            if (target.Model.CurHp <= 0f)
            {
                _combatManager.OnMonsterKilled(target.gameObject);
            }
        }
    }
}