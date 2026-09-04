using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("=== 컴포넌트 참조 ===")]
    [SerializeField] private CharacterAnimationView _animationView;

    [Header("=== 전투 설정 ===")]
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _moveSpeed = 2f;

    public MonsterModel Model { get; private set; }
    public bool IsDead => Model == null || Model.CurHp <= 0f;

    private MonsterData _data;
    private float _attackTimer;

    private void Awake()
    {
        if (_animationView == null)
            _animationView = GetComponent<CharacterAnimationView>();
    }

    public void Setup(MonsterData data)
    {
        _data = data;
        Model = new MonsterModel(data);
        _attackTimer = 0f;
    }

    private void Update()
    {
        if (IsDead) return;

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
                transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            _animationView?.PlayMove(false);
            _animationView?.PlayAttack(true);

            float attackSpeed = _data != null && _data.AttackSpeed > 0f ? _data.AttackSpeed : 1f;
            float attackInterval = 1f / attackSpeed;
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= attackInterval)
            {
                _attackTimer -= attackInterval;
                float attackPower = _data != null ? _data.AttackPower : 10f;
                player.TakeDamage(attackPower);
            }
        }
    }

    public void TakeDamage(float incomingDamage, float attackerAccuracy = 100f)
    {
        if (IsDead) return;

        Model.ChangeCurHp(-incomingDamage);

        if (Model.CurHp <= 0f)
        {
            GameManager.Instance.Combat.OnMonsterKilled(gameObject);
        }
    }
}