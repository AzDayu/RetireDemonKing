using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class ResourceManager : MonoBehaviour
{
    private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _handles = new();
    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();

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

    // 프리랩 로드
    public async UniTask<GameObject> LoadPrefab(string address)
    {
        if (_prefabHandles.TryGetValue(address, out AsyncOperationHandle<GameObject> cachedHandle) && cachedHandle.IsValid())
        {
            return cachedHandle.Result;
        }

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
        try
        {
            GameObject prefab = await handle.ToUniTask();
            _prefabHandles[address] = handle;
            return prefab;
        }
        catch (System.Exception)
        {
            Debug.LogError($"[ResourceManager] 프리팹 로드 실패: {address}");
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
    public void ReleasePrefab(string address)
    {
        if (_prefabHandles.TryGetValue(address, out AsyncOperationHandle<GameObject> handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            _prefabHandles.Remove(address);
        }
    }
}