using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    public MonsterModel Model { get; private set; }

    private const float MoveSpeed = 2f;

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
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}