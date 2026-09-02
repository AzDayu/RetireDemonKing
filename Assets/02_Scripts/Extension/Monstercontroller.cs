using System.Collections;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("=== 애니메이션 뷰 참조 ===")]
    [SerializeField] private CharacterAnimationView _animationView;

    public MonsterModel Model { get; private set; }

    private const float MoveSpeed = 2f;

    private void Awake()
    {
        if (_animationView == null)
        {
            _animationView = GetComponent<CharacterAnimationView>();
        }
    }

    public void Setup(MonsterData data)
    {
        Model = new MonsterModel(data);
        MoveToPlayer();
    }

    private void MoveToPlayer()
    {
        Transform player = PlayerController.Instance;
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        direction.Normalize();
        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 targetPosition = transform.position + direction * 2f;
        StartCoroutine(MoveToPosition(targetPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        _animationView?.PlayMove(true);

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        _animationView?.PlayMove(false);
    }

    public void PlayAttackAnimation(bool isAttacking) => _animationView?.PlayAttack(isAttacking);
    public void PlayDieAnimation() => _animationView?.PlayDie();
}