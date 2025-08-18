using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyEnhance
{
    public string name;
    public int enemyhpbonus;
    public int enemyattackbonus;
    public int enemymovespeedbonus;
    public int enemyattackspeedbonus;
    public int enemyattackrangebonus;
}

[System.Serializable]
public class SpawnArea
{
    public string name;
    public Vector2 center;
    public Vector2 size;
    public int enemyCount;       // 웨이브당 생성 적 수
    public int enemyTowerCount;  // 타워형 적 수
    public bool canProceed = true; // 이 영역으로 진행할 수 있는지 여부
    [Tooltip("이 영역이 활성화될 웨이브 번호들 (비워두면 모든 웨이브에서 활성화)")]
    public List<int> activeWaves = new List<int>();
}

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 영역 설정")]
    public List<SpawnArea> spawnAreas = new List<SpawnArea>();
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("프리팹")]
    public GameObject enemyBasePrefab; // 기지 프리팹
    public List<Enemy> enemyTypes = new List<Enemy>(); // 적 종류

    [Header("웨이브 시스템")]
    public float initialDelay = 2f;
    public bool autoSpawn = true;
    public bool canStartNextWave = true; 
    public bool waitForAllAreasClear = false; 
    
    [Header("웨이브 시간 스케줄")]
    public bool useTimeSchedule = false;
    public List<float> waveStartTimes = new List<float>() { 0f, 20f, 30f };

    [Header("적 강화 설정 (웨이브별)")]
    public List<EnemyEnhance> enemyEnhances = new List<EnemyEnhance>();

    private int currentWave = 1;

    void Start()
    {
        StartCoroutine(WaveSystemCoroutine());
    }

    private IEnumerator WaveSystemCoroutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (autoSpawn)
        {
            // 다음 웨이브 조건 기다리기
            while (!canStartNextWave)
            {
                yield return new WaitForSeconds(1f);
            }

            // 시간 기반 스케줄
            if (useTimeSchedule)
            {
                int waveIndex = currentWave - 1;
                if (waveIndex >= 0 && waveIndex < waveStartTimes.Count)
                {
                    float targetTime = waveStartTimes[waveIndex];
                    yield return new WaitUntil(() => 
                        GameManager.instance != null && GameManager.instance.GetGameTime() >= targetTime
                    );
                }
            }

            EnemyEnhance enhance = GetEnhanceForWave(currentWave);
            Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 시작 - 강화: {enhance.name}");

            // 웨이브와 매치된 영역만 처리
            foreach (var area in spawnAreas)
            {
                // activeWaves가 비어있으면 모든 웨이브에서 활성화
                bool isActive = (area.activeWaves == null || area.activeWaves.Count == 0 
                                 || area.activeWaves.Contains(currentWave));

                if (!isActive) continue;

                // 진행 가능 여부 대기
                while (!area.canProceed)
                {
                    yield return new WaitForSeconds(1f);
                }

                Debug.Log($"[EnemySpawner] 스폰 영역 '{area.name}'에서 웨이브 {currentWave} 처리");

                // 기지 생성
                for (int j = 0; j < area.enemyTowerCount; j++)
                {
                    Vector2 pos = GetRandomPosInArea(area);

                    GameObject baseObj = Instantiate(enemyBasePrefab, pos, Quaternion.identity);
                    spawnPoints.Add(baseObj.transform);

                    EnemyTower enemyTower = baseObj.GetComponent<EnemyTower>();
                    if (enemyTower != null)
                    {
                        enemyTower.enemyTypes = enemyTypes;
                        enemyTower.enhance = enhance;
                        enemyTower.enemyCount = area.enemyCount;
                        enemyTower.StartSpawning();
                    }
                }
            }

      

            currentWave++;
        }
    }

    public Vector2 GetRandomPosInArea(SpawnArea area)
    {
        return area.center + new Vector2(
            Random.Range(-area.size.x / 2f, area.size.x / 2f),
            Random.Range(-area.size.y / 2f, area.size.y / 2f)
        );
    }

    private EnemyEnhance GetEnhanceForWave(int wave)
    {
        int index = wave - 1;
        if (index >= 0 && index < enemyEnhances.Count)
            return enemyEnhances[index];

        return new EnemyEnhance() { name = "기본" };
    }

    public void OnSpawnPointDestroyed(Transform destroyedSpawnPoint)
    {
        spawnPoints.RemoveAll(p => p == null || p == destroyedSpawnPoint);

        if (spawnPoints.Count == 0)
        {
            Debug.Log("[EnemySpawner] 스폰 포인트가 모두 파괴됨 - 웨이브 종료");
            if (currentWave == waveStartTimes.Count) // 마지막 웨이브라면 클리어
                GameManager.instance.TriggerGameClear();
        }
    }
}
