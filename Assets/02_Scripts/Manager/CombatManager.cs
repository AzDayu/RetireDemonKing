using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Boss Timer Settings")]
    [SerializeField] private float _maxBossTime = 30f;
    private float _currentBossTimer;
    private bool _isTimerRunning;

    [Header("Monster Spawn Settings")]
    [SerializeField] private Transform _monsterSpawnPoint;
    [SerializeField] private List<GameObject> _normalMonsterPrefabs;
    [SerializeField] private List<GameObject> _bossMonsterPrefabs;

    [Header("Wave & Target Settings")]
    public const int WaveMaxCount = 10;
    private int _currentKillCount = 0;
    private bool _isBossBattle = false;
    private int _activeStageIndex = 1;

    private Dictionary<string, Queue<GameObject>> _monsterPool = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<GameObject, string> _activeMonsters = new Dictionary<GameObject, string>();

    // StageManager 및 UI에서 구독할 이벤트들
    public event Action OnBattleCleared;
    public event Action OnBattleFailed;
    public event Action<int, int> OnWaveUpdated;
    public event Action<float, float> OnBossTimerUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (_isBossBattle && _isTimerRunning)
        {
            _currentBossTimer -= Time.deltaTime;
            OnBossTimerUpdated?.Invoke(_currentBossTimer, _maxBossTime);

            if (_currentBossTimer <= 0f)
            {
                _isTimerRunning = false;
                DespawnAllActiveMonsters();
                OnBattleFailed?.Invoke(); // 보스전 제한시간 초과 패배 알림
            }
        }
    }

    public void StartNormalBattle(int stageIndex)
    {
        _activeStageIndex = stageIndex;
        _isBossBattle = false;
        _isTimerRunning = false;
        _currentKillCount = 0;

        OnWaveUpdated?.Invoke(_currentKillCount, WaveMaxCount);
        SpawnMonsterForStage(stageIndex, false);
    }

    public void StartBossBattle(int stageIndex)
    {
        _activeStageIndex = stageIndex;
        _isBossBattle = true;
        _currentBossTimer = _maxBossTime;
        _isTimerRunning = true;

        SpawnMonsterForStage(stageIndex, true);
    }

    // 몬스터가 사망했을 때 호출
    public void OnMonsterKilled(GameObject monsterObj)
    {
        if (!DespawnMonster(monsterObj)) return;

        if (_isBossBattle)
        {
            _isTimerRunning = false;
            OnBattleCleared?.Invoke();
        }
        else
        {
            _currentKillCount++;
            OnWaveUpdated?.Invoke(_currentKillCount, WaveMaxCount);

            if (_currentKillCount >= WaveMaxCount)
            {
                OnBattleCleared?.Invoke();
            }
            else
            {
                SpawnMonsterForStage(_activeStageIndex, false);
            }
        }
    }

    private void SpawnMonsterForStage(int stageIndex, bool isBoss)
    {
        List<GameObject> prefabs = isBoss ? _bossMonsterPrefabs : _normalMonsterPrefabs;
        if (prefabs == null || prefabs.Count == 0) return;

        int prefabIndex = (stageIndex - 1) % prefabs.Count;
        GetMonsterFromPool(prefabs[prefabIndex]);
    }

    // 몬스터 풀
    private GameObject GetMonsterFromPool(GameObject prefab)
    {
        string key = prefab.name;

        if (!_monsterPool.ContainsKey(key))
        {
            _monsterPool[key] = new Queue<GameObject>();
        }

        GameObject monster;

        if (_monsterPool[key].Count > 0)
        {
            monster = _monsterPool[key].Dequeue();
        }
        else
        {
            monster = Instantiate(prefab, transform);
        }

        monster.transform.position = _monsterSpawnPoint != null ? _monsterSpawnPoint.position : Vector3.zero;
        monster.transform.rotation = Quaternion.identity;
        monster.SetActive(true);

        _activeMonsters[monster] = key;

        return monster;
    }

    public bool DespawnMonster(GameObject monsterObj)
    {
        if (!_activeMonsters.TryGetValue(monsterObj, out string key))
        {
            Debug.LogWarning($"[CombatManager] 활성 목록에 없는 몬스터 디스폰 시도: {monsterObj.name}");
            return false;
        }

        monsterObj.SetActive(false);
        _activeMonsters.Remove(monsterObj);

        if (!_monsterPool.ContainsKey(key))
        {
            _monsterPool[key] = new Queue<GameObject>();
        }
        _monsterPool[key].Enqueue(monsterObj);

        return true;
    }

    private void DespawnAllActiveMonsters()
    {
        var monsters = new List<GameObject>(_activeMonsters.Keys);
        foreach (var monster in monsters)
        {
            DespawnMonster(monster);
        }
    }

}

