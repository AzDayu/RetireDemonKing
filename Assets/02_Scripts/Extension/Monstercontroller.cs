using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public MonsterModel Model { get; private set; }

    public void Setup(MonsterData data)
    {
        Model = new MonsterModel(data);
    }
}