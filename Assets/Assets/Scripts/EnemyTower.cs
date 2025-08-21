using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyTower : MonoBehaviour
{

    public GameObject hpBarPrefab; // HP바 프리팹
    public int maxHealth = 100;
    public int currentHealth;

    public EnemySpawner EnemySpawner;
    public List<Enemy> enemyTypes = new List<Enemy>();
    public GameObject enemyPrefab;
    public EnemyEnhance enhance;
    public float spawnInterval = 2f;
    public int maxEnemiesAroundBase = 7;
    public float initialDelay = 2f;

    // HP바 관련 변수
    private GameObject hpBarInstance; // 생성된 HP바 인스턴스
    private hpbar hpBarComponent; // HP바 컴포넌트

    private bool isSpawning = false;
    private int wave = 0;
    public int enemyCount = 0;

    void Awake()
    {
        currentHealth = maxHealth;
        
        // EnemySpawner가 할당되지 않은 경우 자동으로 찾기
        if (EnemySpawner == null)
        {
            EnemySpawner = FindObjectOfType<EnemySpawner>();
            if (EnemySpawner == null)
            {
                Debug.LogWarning($"[EnemyTower] EnemySpawner를 찾을 수 없습니다: {gameObject.name}");
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // HP바 업데이트 및 데미지 숫자 표시
        if (hpBarComponent != null)
        {
            hpBarComponent.decreaseHp(damageAmount);
            // hpbar의 currentHp를 EnemyTower의 currentHealth와 동기화
            currentHealth = (int)hpBarComponent.currentHp;
        }
        if (currentHealth <= 0)
        {
            // EnemySpawner가 null이 아닌지 확인
            if (EnemySpawner != null)
            {
                EnemySpawner.OnSpawnPointDestroyed(transform.parent);
            }
            else
            {
                Debug.LogWarning($"[EnemyTower] EnemySpawner가 할당되지 않았습니다: {gameObject.name}");
            }
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // HP바 생성 및 설정
        CreateHpBar();
    }

    // HP바 생성 메서드
    private void CreateHpBar()
    {
        if (hpBarPrefab == null)
        {
            Debug.LogWarning("[EnemyTower] hpbar 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // HP바 인스턴스를 부모 없이 생성
        hpBarInstance = Instantiate(hpBarPrefab);
        
        // 캔버스를 월드 스페이스로 설정
        Canvas canvas = hpBarInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 6;
        }

        // HP바 컴포넌트 가져오기
        hpBarComponent = hpBarInstance.GetComponentInChildren<hpbar>();
        if (hpBarComponent == null)
        {
            Debug.LogWarning("[EnemyTower] hpbar 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 월드 좌표로 위치 설정 (적 기지 아래쪽)
        Vector3 targetPosition = transform.position + new Vector3(0, -0.8f, 0);
        hpBarInstance.transform.position = targetPosition;

        // HP바 초기화 및 색상 설정
        hpBarComponent.maxHp = maxHealth;
        hpBarComponent.fullHpColor = Color.green; // 풀 HP일 때 초록색
        hpBarComponent.lowHpColor = Color.red;    // 낮은 HP일 때 빨간색
        hpBarComponent.SetHp(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnLoop());
        }
    }

    private IEnumerator SpawnLoop()

    {
            yield return new WaitForSeconds(initialDelay);



        while (isSpawning)
        {
            if (CountEnemiesNearby() < maxEnemiesAroundBase)
                SpawnEnemy();
            enemyCount--;
            if (enemyCount <= 0)
                isSpawning = false;
            
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyTypes.Count == 0) return;

        Enemy enemyType = enemyTypes[Random.Range(0, enemyTypes.Count)];
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 1.5f;

        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        SpawnedEnemy spawned = enemyInstance.GetComponent<SpawnedEnemy>();

        if (spawned != null)
        {
            spawned.enemyData = enemyType;
            spawned.InitializeEnemy();
            ApplyEnhance(spawned, enhance);
        }
    }

    private void ApplyEnhance(SpawnedEnemy enemy, EnemyEnhance e)

    {
        enemy.currentHp += e.enemyhpbonus;
        enemy.attackDamage += e.enemyattackbonus;
        enemy.moveSpeed += e.enemymovespeedbonus;
        enemy.attackSpeed += e.enemyattackspeedbonus;
        enemy.attackRange += e.enemyattackrangebonus;
    }

    private int CountEnemiesNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f);
        int count = 0;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy")) count++;
        }
        return count;
    }

    // 오브젝트가 파괴될 때 HP바도 함께 파괴
    void OnDestroy()
    {
        if (hpBarInstance != null)
        {
            Destroy(hpBarInstance);
        }
    }
}
