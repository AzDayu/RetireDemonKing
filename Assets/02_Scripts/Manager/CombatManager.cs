using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Boss Timer Settings")]
    [SerializeField] private float _maxBossTime = 30f;
    private float _currentBossTimer;
    private bool _isTimerRunning;

    [Header("Monster Spawn Settings")]
    [SerializeField] private Transform[] _monsterSpawnPoints;
    [SerializeField] private MonsterSpawnTable _monsterSpawnTable;
    private bool _isBossBattle = false;

    // StageManager 및 UI에서 구독할 이벤트들
    public event Action OnBattleCleared;
    public event Action OnBattleFailed;
    public event Action<int, int> OnWaveUpdated;
    public event Action<float, float> OnBossTimerUpdated;
    public event Action<MonsterController> OnBossSpawned;

    private Queue<string> _waveQueue = new Queue<string>();
    private Dictionary<string, Queue<GameObject>> _monsterPool = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<GameObject, string> _activeMonsters = new Dictionary<GameObject, string>();

    private int _currentKillCount = 0;
    private int _currentWaveTotal = 0;

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
                OnBattleFailed?.Invoke();
            }
        }
    }

    public void StartNormalBattle(int stageIndex)
    {
        _isBossBattle = false;
        _isTimerRunning = false;
        _currentKillCount = 0;

        StageTheme theme = stageIndex.GetTheme(GameManager.Instance.Stage.StagesForChange);
        BuildWaveQueue(theme);

        OnWaveUpdated?.Invoke(_currentKillCount, _currentWaveTotal);
        SpawnNextWaveMonster();
    }

    public void StartBossBattle(int stageIndex)
    {
        _isBossBattle = true;
        _currentBossTimer = _maxBossTime;
        _isTimerRunning = true;

        StageTheme theme = stageIndex.GetTheme(GameManager.Instance.Stage.StagesForChange);
        string bossId = _monsterSpawnTable.GetBossMonsterId(theme);
        if (bossId == null) return;

        SpawnMonsterById(bossId);
    }

    public MonsterController GetActiveMonster()
    {
        foreach (var kv in _activeMonsters)
        {
            return kv.Key.GetComponent<MonsterController>();
        }
        return null;
    }

    // 몬스터가 사망했을 때 호출
    public void OnMonsterKilled(GameObject monsterObj)
    {
        if (!DespawnMonster(monsterObj)) return;
        Debug.Log($"[CombatManager] 몬스터 사망 처리됨: {monsterObj.name}");
        if (_isBossBattle)
        {
            _isTimerRunning = false;
            OnBattleCleared?.Invoke();
        }
        else
        {
            _currentKillCount++;
            OnWaveUpdated?.Invoke(_currentKillCount, _currentWaveTotal);

            if (_currentKillCount >= _currentWaveTotal)
            {
                Debug.Log($"[CombatManager] 웨이브 전체 처치 완료 ({_currentKillCount}/{_currentWaveTotal}) -> 전투 클리어");
                OnBattleCleared?.Invoke();
            }
            else
            {
                SpawnNextWaveMonster();
            }
        }
    }

    private void BuildWaveQueue(StageTheme theme)
    {
        _waveQueue.Clear();

        List<MonsterSpawnEntry> entries = _monsterSpawnTable.GetMonsters(theme);
        List<string> expanded = new List<string>();

        foreach (var entry in entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                expanded.Add(entry.monsterId);
            }
        }

        if (expanded.Count == 0)
        {
            Debug.LogWarning($"[CombatManager] 테마 {theme}에 등록된 몬스터가 없습니다. MonsterSpawnTable 설정을 확인하세요.");
            return;
        }

        GameUtil.Shuffle(expanded); //셔플

        foreach (var id in expanded)
        {
            _waveQueue.Enqueue(id);
        }

        _currentWaveTotal = expanded.Count;
    }

    private void SpawnNextWaveMonster()
    {
        if (_waveQueue.Count == 0)
        {
            Debug.LogWarning("[CombatManager] 웨이브 큐가 비어있는데 스폰이 호출되었습니다.");
            return;
        }
        string monsterId = _waveQueue.Dequeue();
        SpawnMonsterById(monsterId);
    }

    private async void SpawnMonsterById(string monsterId)
    {
        MonsterData data = GameManager.Instance.Data.GetMonsterData(monsterId);
        if (data == null)
        {
            Debug.LogWarning($"[CombatManager] MonsterData를 찾을 수 없음: {monsterId}");
            return;
        }

        GameObject prefab = await GameManager.Instance.Resource.LoadPrefab(data.PrefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[CombatManager] 프리팹 로드 실패: {data.PrefabName}");
            return;
        }

        SpawnMonsterFromPool(prefab, data, monsterId);
    }

    // 몬스터 풀
    private void SpawnMonsterFromPool(GameObject prefab, MonsterData data, string monsterId)
    {
        if (!_monsterPool.ContainsKey(monsterId))
        {
            _monsterPool[monsterId] = new Queue<GameObject>();
        }

        GameObject monster;
        if (_monsterPool[monsterId].Count > 0)
        {
            monster = _monsterPool[monsterId].Dequeue();
        }
        else
        {
            monster = Instantiate(prefab, transform);
        }

        Transform spawnPoint = GameUtil.GetRandomElement(_monsterSpawnPoints);
        monster.transform.position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        monster.transform.rotation = Quaternion.identity;
        monster.SetActive(true);

        _activeMonsters[monster] = monsterId;

        MonsterController controller = monster.GetComponent<MonsterController>();
        controller?.Setup(data);

        if (_isBossBattle)
        {
            OnBossSpawned?.Invoke(controller);
        }
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

    public void TriggerBattleFailed()
    {
        _isTimerRunning = false;
        DespawnAllActiveMonsters();
        OnBattleFailed?.Invoke();
    }
}
