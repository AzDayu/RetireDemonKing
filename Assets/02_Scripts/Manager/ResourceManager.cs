using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Inst { get; private set; }

    private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _handles = new();

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    // 스프라이트 로드
    public async UniTask<Sprite> LoadSprite(string address)
    {
        // 캐시 확인
        if (_handles.TryGetValue(address, out AsyncOperationHandle<Sprite> cachedHandle) && cachedHandle.IsValid())
        {
            return cachedHandle.Result;
        }

        // 로드 실행
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
        try
        {
            Sprite sprite = await handle.ToUniTask();
            _handles[address] = handle;
            return sprite;
        }
        catch (System.Exception)
        {
            Debug.LogError($"[ResourceManager] 스프라이트 로드 실패: {address}");

            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return null;
        }
    }

    // 메모리 해제
    public void Release(string address)
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle<Sprite> handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            _handles.Remove(address);
        }
    }
}