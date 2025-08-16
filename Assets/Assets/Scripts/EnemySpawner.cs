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
    public float waveCooldown = 30f;
    public float initialDelay = 2f;
    public bool autoSpawn = true;
    public bool canStartNextWave = true; // 다음 웨이브를 시작할 수 있는지 여부
    public bool waitForAllAreasClear = false; // 모든 영역이 클리어될 때까지 기다릴지 여부

    [Header("적 강화 설정 (웨이브별)")]
    public List<EnemyEnhance> enemyEnhances = new List<EnemyEnhance>();

    private int currentWave = 1; // 0에서 1로 변경
    private int currentAreaIndex = 0; // 현재 처리 중인 영역 인덱스

    void Start()
    {
        StartCoroutine(WaveSystemCoroutine());
    }

    private IEnumerator WaveSystemCoroutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (autoSpawn)
        {
            // 다음 웨이브 시작 조건 확인
            while (!canStartNextWave)
            {
                Debug.Log("[EnemySpawner] 다음 웨이브 시작 조건을 기다리는 중...");
                yield return new WaitForSeconds(1f);
            }

            EnemyEnhance enhance = GetEnhanceForWave(currentWave);
            Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 시작 - 강화: {enhance.name}");

            // 각 스폰 영역을 순차적으로 처리
            for (int i = 0; i < spawnAreas.Count; i++)
            {
                var area = spawnAreas[i];
                currentAreaIndex = i;
                
                // 영역 진행 가능 여부 확인
                while (!area.canProceed)
                {
                    Debug.Log($"[EnemySpawner] 스폰 영역 '{area.name}' 진행 조건을 기다리는 중...");
                    yield return new WaitForSeconds(1f);
                }

                Debug.Log($"[EnemySpawner] 스폰 영역 '{area.name}' 처리 중...");
                
                // 현재 영역에 기지 생성
                for (int j = 0; j < area.enemyTowerCount; j++)
                {
                    Vector2 pos = GetRandomPosInArea(area);
                    
                    GameObject baseObj = Instantiate(enemyBasePrefab, pos, Quaternion.identity);
                    spawnPoints.Add(baseObj.transform);
                    
                    // 기지 스크립트에 데이터 전달
                    EnemyTower enemyTower = baseObj.GetComponent<EnemyTower>();
                    if (enemyTower != null)
                    {
                        enemyTower.enemyTypes = enemyTypes;
                        enemyTower.enhance = enhance;
                        enemyTower.StartSpawning();
                        enemyTower.enemyCount = area.enemyCount;
                    }
                }
                    yield return new WaitForSeconds(waveCooldown);
                   
                
            }

            // 모든 영역이 클리어될 때까지 기다려야 하는 경우
            if (waitForAllAreasClear)
            {
                Debug.Log("[EnemySpawner] 모든 영역 클리어를 기다리는 중...");
                while (spawnPoints.Count > 0)
                {
                    yield return new WaitForSeconds(1f);
                }
            }
            
            Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 완료. 다음 웨이브까지 {waveCooldown}초 대기");
            yield return new WaitForSeconds(waveCooldown);
            currentWave++;
        }
    }

    private Vector2 GetRandomPosInArea(SpawnArea area)
    {
        return area.center + new Vector2(
            Random.Range(-area.size.x / 2f, area.size.x / 2f),
            Random.Range(-area.size.y / 2f, area.size.y / 2f)
        );
    }

    private EnemyEnhance GetEnhanceForWave(int wave)
    {
        // wave가 1부터 시작하므로 인덱스는 0부터 시작
        int index = wave - 1;
        
        // 안전한 인덱스 체크
        if (index >= 0 && index < enemyEnhances.Count && enemyEnhances.Count > 0)
        {
            return enemyEnhances[index];
        }
        
        // 기본 강화 설정 반환
        return new EnemyEnhance() { 
            name = "기본",
            enemyhpbonus = 0,
            enemyattackbonus = 0,
            enemymovespeedbonus = 0,
            enemyattackspeedbonus = 0,
            enemyattackrangebonus = 0
        };
    }

   
   public void OnSpawnPointDestroyed(Transform destroyedSpawnPoint) { 
    int removedCount = spawnPoints.RemoveAll(point => point == null || point == destroyedSpawnPoint); 
        if (spawnPoints.Count==0)
        {
            Debug.Log($"[EnemySpawner] 스폰 포인트가 모두 파괴되었습니다. 웨이브 종료");
            
        }
} 

    public int GetRemainingSpawnPoints() { 
        spawnAreas.RemoveAll(point => point == null); 
        return spawnAreas.Count; 
    }    

    // 웨이브 진행 제어 메서드들
    public void SetCanStartNextWave(bool canStart)
    {
        canStartNextWave = canStart;
        Debug.Log($"[EnemySpawner] 다음 웨이브 시작 가능: {canStart}");
    }

    public void SetAreaCanProceed(int areaIndex, bool canProceed)
    {
        if (areaIndex >= 0 && areaIndex < spawnAreas.Count)
        {
            spawnAreas[areaIndex].canProceed = canProceed;
            Debug.Log($"[EnemySpawner] 영역 '{spawnAreas[areaIndex].name}' 진행 가능: {canProceed}");
        }
    }

   
    public void SetWaitForAllAreasClear(bool waitForAll)
    {
        waitForAllAreasClear = waitForAll;
        Debug.Log($"[EnemySpawner] 모든 영역 클리어 대기: {waitForAll}");
    }

    public bool IsCurrentArea(int areaIndex)
    {
        return currentAreaIndex == areaIndex;
    }

    public string GetCurrentAreaName()
    {
        if (currentAreaIndex >= 0 && currentAreaIndex < spawnAreas.Count)
            return spawnAreas[currentAreaIndex].name;
        return "없음";
    }
        
    
}
