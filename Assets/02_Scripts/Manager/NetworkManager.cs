using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; set; }

    //public InventoryNetworkService InventoryService { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"NetworkManager: 중복된 NetworkManager를 제거합니다. Object: {gameObject.name}", this);
            Destroy(this);
            return;
        }

        Instance = this;
        InitNetworkService();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitNetworkService()
    {
        //InventoryService = new InventoryNetworkService();
    }
}