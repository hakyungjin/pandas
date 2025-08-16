using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyTower : MonoBehaviour
{

    public hpbar hpBar;
    public int maxHealth = 100;
    public int currentHealth;
    public EnemySpawner EnemySpawner;
    public List<Enemy> enemyTypes = new List<Enemy>();
    public GameObject enemyPrefab;
    public EnemyEnhance enhance;
    public float spawnInterval = 2f;
    public int maxEnemiesAroundBase = 7;
    public float initialDelay = 2f;

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
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        hpBar.decreaseHp(damageAmount);
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
        hpBar.SetHp(currentHealth);
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
}
