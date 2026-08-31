using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Transform Instance { get; private set; }

    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private LayerMask _monsterLayer;
    private float _attackTimer;

    private void Awake()
    {
        Instance = transform;
    }

    private void Update()
    {
        float attackSpeed = GameManager.Instance.Growth.GetStat(StatType.AttackSpeed);
        if (attackSpeed <= 0f) attackSpeed = 1f; //// 삭제
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
        float damage = GameManager.Instance.Growth.GetStat(StatType.Attack);
        if (damage <= 0f) damage = 50; //// 삭제

        foreach (var hit in hits)
        {
            MonsterController target = hit.GetComponent<MonsterController>();
            if (target == null || target.Model == null) continue;

            target.Model.ChangeCurHp(-damage);
            // 임시 로그. 나중에 제거
            Debug.Log($"[PlayerController] 공격! 데미지: {damage}, 남은 HP: {target.Model.CurHp}");

            if (target.Model.CurHp <= 0f)
            {
                GameManager.Instance.Combat.OnMonsterKilled(target.gameObject);
            }
        }
    }
}