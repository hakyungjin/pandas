using UnityEngine;

public class EnemyTower : MonoBehaviour
{

    public hpbar hpBar;
    public int maxHealth = 100;
    public int currentHealth;
    public EnemySpawner EnemySpawner;

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
}
