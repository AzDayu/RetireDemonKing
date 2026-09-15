using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("=== 컴포넌트 참조 ===")]
    [SerializeField]
    private CharacterAnimationView _animationView;

    [Header("=== 전투 설정 ===")]
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _moveSpeed = 2f;

    public MonsterModel Model { get; private set; }

    public bool IsDead => Model == null || Model.CurHp <= 0f;

    private MonsterData _data;

    private void Awake()
    {
        if (_animationView == null)
        {
            _animationView = GetComponent<CharacterAnimationView>();
        }
    }

    private void OnEnable()
    {
        if (_animationView != null)
        {
            _animationView.OnAttackHit += HandleAttackHit;
        }
    }

    private void OnDisable()
    {
        if (_animationView != null)
        {
            _animationView.OnAttackHit -= HandleAttackHit;
        }
    }

    public void Setup(MonsterData data, int stageIndex, float hpIncreasePerStage, float attackIncreasePerStage)
    {
        if (data == null)
        {
            _data = null;
            Model = null;

            Debug.LogError("[MonsterController] 몬스터 데이터가 없습니다.");

            return;
        }

        _data = data;

        Model = new MonsterModel(data);

        int safeStage = Mathf.Max(1, stageIndex);
        int stageOffset = safeStage - 1;

        float hpMultiplier = 1f + stageOffset * Mathf.Max(0f, hpIncreasePerStage);

        float attackMultiplier = 1f + stageOffset * Mathf.Max(0f, attackIncreasePerStage);

        Model.MaxHp = Mathf.Max(1f, data.MaxHp * hpMultiplier);

        Model.CurHp = Model.MaxHp;

        Model.AttackPower = Mathf.Max(0f, data.AttackPower * attackMultiplier);

        Debug.Log(
            $"[MonsterController] 몬스터 생성 | " +
            $"Stage: {safeStage} | " +
            $"ID: {data.MonsterId} | " +
            $"HP: {Model.MaxHp:F1} | " +
            $"ATK: {Model.AttackPower:F1}"
        );
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        PlayerController player = PlayerController.Instance;

        if (player == null || player.IsDead)
        {
            _animationView?.PlayAttack(false);
            _animationView?.PlayMove(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > _attackRange)
        {
            _animationView?.PlayAttack(false);
            _animationView?.PlayMove(true);

            Vector3 dir = (player.transform.position - transform.position).normalized;

            dir.y = 0f;

            transform.position += dir * (_moveSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        else
        {
            _animationView?.PlayMove(false);
            _animationView?.PlayAttack(true);

            float attackSpeed = _data != null && _data.AttackSpeed > 0f ? _data.AttackSpeed : 1f;

            _animationView?.SetAnimationSpeed(attackSpeed);
        }
    }

    public void TakeDamage(
        float incomingDamage,
        float attackerAccuracy = 100f)
    {
        if (IsDead)
        {
            return;
        }

        Model.ChangeCurHp(-incomingDamage);

        if (Model.CurHp <= 0f)
        {
            EquipmentDropFlow.ProcessMonsterKill(() => GameManager.Instance.Combat.OnMonsterKilled(gameObject));
        }
    }

    private void HandleAttackHit()
    {
        if (IsDead)
        {
            return;
        }

        PlayerController player = PlayerController.Instance;

        if (player == null || player.IsDead)
        {
            return;
        }

        float attackPower = Mathf.Max(0f, Model.AttackPower);

        player.TakeDamage(attackPower);

        Debug.Log(
            $"[Monster 공격 성공] " +
            $"대상: {player.name} | " +
            $"공격력: {attackPower:F1} | " +
            $"플레이어 남은 HP: {player.CurHp:F1}"
        );
    }
}