using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyBaseWave
{
    public string waveName = "웨이브";
    public Vector3[] spawnPositions = new Vector3[0];
}

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public List<Transform> spawnPoints = new List<Transform>(); // 스폰 위치들
    public List<Enemy> enemyTypes = new List<Enemy>(); // 스폰할 몬스터 타입들
    public GameObject enemyPrefab; // 몬스터 프리팹
    
    [Header("웨이브 시스템")]
    public int enemiesPerWave = 5; // 웨이브당 적 수
    public float waveCooldown = 10f; // 웨이브 간 대기 시간 (초)
    public float spawnIntervalInWave = 1f; // 웨이브 내 적들 사이의 스폰 간격 (초)
    public float initialDelay = 2f; // 게임 시작 후 첫 웨이브까지의 대기 시간
    
    [Header("스폰 옵션")]
    public bool autoSpawn = true; // 자동 스폰 여부
    public int maxEnemiesOnScreen = 20; // 화면에 동시에 존재할 수 있는 최대 몬스터 수
    
    [Header("게임 관리")]
    public GameManager gameManager; // 게임 매니저 참조
    
    [Header("적 기지 시스템")]
    public GameObject enemyBasePrefab; // 적 기지 프리팹
    public List<EnemyBaseWave> baseSpawnWaves = new List<EnemyBaseWave>(); // 적 기지 생성 웨이브들

    public float firstBaseSpawnTime = 60f; // 첫 번째 기지 생성 시간 (1분)
    public float secondBaseSpawnTime = 240f; // 두 번째 기지 생성 시간 (4분)
    
    // 웨이브 관리 변수들
    private int currentWave = 0; // 현재 웨이브 번호
    private bool isWaveActive = false; // 웨이브 진행 중인지 여부
    private bool isWaveCooldown = false; // 웨이브 쿨다운 중인지 여부
    private int enemiesSpawnedInCurrentWave = 0; // 현재 웨이브에서 스폰된 적 수
    
    private bool hasTriggeredGameOver = false; // 게임오버 중복 실행 방지
    private bool firstBaseSpawned = false; // 첫 번째 기지 생성 여부
    private bool secondBaseSpawned = false; // 두 번째 기지 생성 여부
    
    void Start()
    {
        StartCoroutine(BaseSpawnTimerCoroutine());
        StartCoroutine(WaveSystemCoroutine());
        Debug.Log("[EnemySpawner] 웨이브 기반 몬스터 스폰 시스템 초기화 완료");
    }
    
    void Update()
    {
        // 웨이브 시스템에서는 코루틴으로 관리하므로 Update에서는 특별한 작업 없음
        // 필요시 UI 업데이트나 디버그 정보 표시 등에 사용 가능
    }
    
    // 웨이브 시스템 메인 코루틴
    private IEnumerator WaveSystemCoroutine()
    {
        // 초기 대기 시간
        yield return new WaitForSeconds(initialDelay);
        
        while (autoSpawn)
        {
            // 웨이브 시작
            currentWave++;
            yield return StartCoroutine(ExecuteWave());
            
            // 웨이브 간 쿨다운
            if (autoSpawn) // 여전히 자동 스폰이 활성화되어 있다면
            {
                isWaveCooldown = true;
                Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 완료. {waveCooldown}초 후 다음 웨이브 시작...");
                yield return new WaitForSeconds(waveCooldown);
                isWaveCooldown = false;
            }
        }
    }
    
    // 개별 웨이브 실행 코루틴
    private IEnumerator ExecuteWave()
    {
        isWaveActive = true;
        enemiesSpawnedInCurrentWave = 0;
        
        Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 시작! ({enemiesPerWave}마리 생성 예정)");
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            // 최대 몬스터 수 체크
            if (GetEnemyCount() >= maxEnemiesOnScreen)
            {
                Debug.Log($"[EnemySpawner] 웨이브 {currentWave}: 최대 몬스터 수 도달로 대기 중...");
                
                // 몬스터 수가 줄어들 때까지 대기
                while (GetEnemyCount() >= maxEnemiesOnScreen && autoSpawn)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                if (!autoSpawn) break; // 스폰이 중지되었다면 웨이브 종료
            }
            
            // 몬스터 스폰
            SpawnEnemy();
            enemiesSpawnedInCurrentWave++;
            
            // 마지막 몬스터가 아니라면 대기
            if (i < enemiesPerWave - 1 && autoSpawn)
            {
                yield return new WaitForSeconds(spawnIntervalInWave);
            }
        }
        
        Debug.Log($"[EnemySpawner] 웨이브 {currentWave} 스폰 완료! (실제 생성: {enemiesSpawnedInCurrentWave}마리)");
        isWaveActive = false;
    }
    
    // 적 기지 생성 타이머 코루틴
    private IEnumerator BaseSpawnTimerCoroutine()
    {
        // 1분 후 첫 번째 웨이브의 적 기지들 생성
        yield return new WaitForSeconds(firstBaseSpawnTime);
        SpawnEnemyBasesForWave(0);
        
        // 4분 후 두 번째 웨이브의 적 기지들 생성 (총 4분, 첫 번째 기지 생성 후 3분 대기)
        yield return new WaitForSeconds(secondBaseSpawnTime - firstBaseSpawnTime);
        SpawnEnemyBasesForWave(1);
    }
    
    // 특정 웨이브의 모든 적 기지들을 생성
    private void SpawnEnemyBasesForWave(int waveIndex)
    {
        if (enemyBasePrefab == null)
        {
            Debug.LogError("[EnemySpawner] 적 기지 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        if (waveIndex >= baseSpawnWaves.Count)
        {
            Debug.LogError($"[EnemySpawner] 웨이브 인덱스가 범위를 벗어났습니다. 인덱스: {waveIndex}, 최대: {baseSpawnWaves.Count - 1}");
            return;
        }
        
        EnemyBaseWave wave = baseSpawnWaves[waveIndex];
        Vector3[] waveSpawnPositions = wave.spawnPositions;
        
        
        // 해당 웨이브의 모든 스폰 위치에 기지 생성
        for (int i = 0; i < waveSpawnPositions.Length; i++)
        {
            Vector3 spawnPosition = waveSpawnPositions[i];
            
            // 적 기지 생성
            GameObject newBase = Instantiate(enemyBasePrefab, spawnPosition, Quaternion.identity);
            
            // 새로 생성된 기지의 Transform을 스폰 포인트 리스트에 추가
            Transform baseTransform = newBase.transform;
            spawnPoints.Add(baseTransform);
            enemiesPerWave++;//기지 생성 시 적 수 증가
            
        }
        
        
       
       
        
        // 기지 생성 상태 업데이트
        if (waveIndex == 0)
            firstBaseSpawned = true;
        else if (waveIndex == 1)
            secondBaseSpawned = true;
    }
    
    // 개별 적 기지 생성 (기존 메서드 유지 - 호환성용)
    private void SpawnNewEnemyBase(int baseIndex)
    {
        if (enemyBasePrefab == null)
        {
            Debug.LogError("[EnemySpawner] 적 기지 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        if (baseIndex >= baseSpawnWaves.Count)
        {
            Debug.LogError($"[EnemySpawner] 기지 생성 위치 인덱스가 범위를 벗어났습니다. 인덱스: {baseIndex}, 최대: {baseSpawnWaves.Count - 1}");
            return;
        }
        
        EnemyBaseWave wave = baseSpawnWaves[baseIndex];
        Vector3[] spawnPositions = wave.spawnPositions;
        
        // 적 기지 생성
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            Vector3 spawnPosition = spawnPositions[i];
            GameObject newBase = Instantiate(enemyBasePrefab, spawnPosition, Quaternion.identity);
            Transform baseTransform = newBase.transform;
            spawnPoints.Add(baseTransform);
        }

        
        
        // 기지 생성 상태 업데이트
        if (baseIndex == 0)
            firstBaseSpawned = true;
        else if (baseIndex == 1)
            secondBaseSpawned = true;
    }
    
    public void SpawnEnemy()
    {
        // 화면에 있는 몬스터 수 체크
        if (GetEnemyCount() >= maxEnemiesOnScreen)
        {
            Debug.Log("[EnemySpawner] 최대 몬스터 수에 도달하여 스폰을 건너뜁니다.");
            return;
        }
        
        // 스폰 위치 선택
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {   
            Debug.Log("[EnemySpawner] 스폰 포인트가 모두 파괴되어 스폰을 중지합니다.");
            return;
        }
        
        
        // 몬스터 타입 선택
        Enemy enemyType = GetRandomEnemyType();
        if (enemyType == null)
        {
            Debug.LogError("[EnemySpawner] 몬스터 타입이 설정되지 않았습니다!");
            return;
        }
        
        // 몬스터 생성
        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        SpawnedEnemy spawnedEnemy = enemyInstance.GetComponent<SpawnedEnemy>();
        
        if (spawnedEnemy != null)
        {
            spawnedEnemy.enemyData = enemyType;
            Debug.Log($"[EnemySpawner] {enemyType.name} 몬스터가 {spawnPoint.name}에서 생성되었습니다.");
        }
        else
        {
            Debug.LogError("[EnemySpawner] SpawnedEnemy 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        // null인 스폰 포인트들을 리스트에서 제거
        spawnPoints.RemoveAll(point => point == null);
        
        if (spawnPoints.Count == 0)
        {
            // 스폰 포인트가 모두 파괴되었을 때 게임오버 처리
            if (!hasTriggeredGameOver && gameManager != null)
            {
                hasTriggeredGameOver = true;
                Debug.Log("[EnemySpawner] 모든 스폰 포인트가 파괴되어 게임오버를 실행합니다!");
                gameManager.OnAllSpawnPointsDestroyed();
            }
            return null;
        }
        
        // 유효한 스폰 포인트에서 랜덤 선택
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedPoint = spawnPoints[randomIndex];
        
        // 선택된 포인트가 null인지 다시 한번 체크 (안전장치)
        if (selectedPoint == null)
        {
            Debug.LogWarning("[EnemySpawner] 선택된 스폰 포인트가 null입니다. 다시 시도합니다.");
            return GetRandomSpawnPoint(); // 재귀 호출로 다시 시도
        }
        
        return selectedPoint;
    }
    
    private Enemy GetRandomEnemyType()
    {
        if (enemyTypes.Count == 0) return null;
        return enemyTypes[Random.Range(0, enemyTypes.Count)];
    }
    
    private int GetEnemyCount()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
    
    // 수동으로 몬스터 스폰 (디버그용)
    public void SpawnEnemyManually()
    {
        SpawnEnemy();
    }
    
    // 웨이브 시스템 시작/정지
    public void StartSpawning()
    {
        if (!autoSpawn)
        {
            autoSpawn = true;
            StartCoroutine(WaveSystemCoroutine());
            Debug.Log("[EnemySpawner] 웨이브 시스템을 시작합니다.");
        }
    }
    
    public void StopSpawning()
    {
        autoSpawn = false;
        isWaveActive = false;
        isWaveCooldown = false;
        Debug.Log("[EnemySpawner] 웨이브 시스템을 중지합니다.");
    }
    
    // 웨이브 정보 조회 메서드들
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public bool IsWaveActive()
    {
        return isWaveActive;
    }
    
    public bool IsWaveCooldown()
    {
        return isWaveCooldown;
    }
    
    public int GetEnemiesSpawnedInCurrentWave()
    {
        return enemiesSpawnedInCurrentWave;
    }
    
    public string GetWaveStatus()
    {
        if (isWaveActive)
            return $"웨이브 {currentWave} 진행 중 ({enemiesSpawnedInCurrentWave}/{enemiesPerWave})";
        else if (isWaveCooldown)
            return $"웨이브 {currentWave} 완료 - 쿨다운 중";
        else
            return "대기 중";
    }
    
    // 특정 위치에 특정 몬스터 스폰
    public void SpawnEnemyAtPosition(Enemy enemyType, Vector3 position)
    {
        if (enemyType == null)
        {
            Debug.LogError("[EnemySpawner] 몬스터 타입이 null입니다!");
            return;
        }
        
        GameObject enemyInstance = Instantiate(enemyPrefab, position, Quaternion.identity);
        SpawnedEnemy spawnedEnemy = enemyInstance.GetComponent<SpawnedEnemy>();
        
        if (spawnedEnemy != null)
        {
            spawnedEnemy.enemyData = enemyType;
            Debug.Log($"[EnemySpawner] {enemyType.name} 몬스터가 지정된 위치에서 생성되었습니다.");
        }
    }
    
    // 스폰 포인트가 파괴될 때 호출되는 메서드
    public void OnSpawnPointDestroyed(Transform destroyedSpawnPoint)
    {
        // null인 스폰 포인트들을 모두 제거
        int removedCount = spawnPoints.RemoveAll(point => point == null || point == destroyedSpawnPoint);
        
        if (removedCount > 0)
        {
            Debug.Log($"[EnemySpawner] 스폰 포인트가 파괴되었습니다. 제거된 개수: {removedCount}, 남은 개수: {spawnPoints.Count}");
            
            // 스폰 포인트가 모두 파괴되었는지 확인
            if (spawnPoints.Count == 0 && !hasTriggeredGameOver && gameManager != null)
            {
                hasTriggeredGameOver = true;
                StopSpawning();
                gameManager.OnAllSpawnPointsDestroyed();
            }
        }
    }
    
    // 현재 남은 스폰 포인트 개수 반환
    public int GetRemainingSpawnPoints()
    {
        // null인 스폰 포인트들을 제거하고 실제 개수 반환
        spawnPoints.RemoveAll(point => point == null);
        return spawnPoints.Count;
    }
} 