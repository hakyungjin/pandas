using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float lifetime = 5f; // 총알이 자동으로 파괴되는 시간

    private Vector3 direction;
    private float timer;
    private float speed;
    private int damage;

    void Start()
    {
        timer = 0f;
        Debug.Log($"[Bullet] Bullet 시작됨: {gameObject.name}");
        // BoxCollider2D 추가 (충돌 감지용)
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void Update()
    {
        // 총알 이동
        transform.Translate(direction * speed * Time.deltaTime);

        // 수명 체크
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Debug.Log($"[Bullet] 총알 수명 만료로 파괴됨: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector3 dir, float bulletSpeed, int bulletDamage)
    {
        Debug.Log($"[Bullet] Initialize 호출됨 - 방향: {dir}, 속도: {bulletSpeed}, 데미지: {bulletDamage}");
        direction = dir.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        Debug.Log($"[Bullet] 초기화 완료 - 방향: {direction}, 속도: {speed}, 데미지: {damage}");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"[Bullet] 충돌 감지: {other.gameObject.name}, 태그: {other.gameObject.tag}, 레이어: {other.gameObject.layer}, 총알 위치: {transform.position}");
        
        // 적과 충돌했는지 확인 (태그 기준)
        if (other.gameObject.CompareTag("Enemy"))
        {
            string enemyName = other.gameObject.name; // 삭제되기 전에 이름 저장
            Debug.Log($"[Bullet] 적과 충돌! 적 이름: {enemyName}");
            
            SpawnedEnemy enemy = other.gameObject.GetComponent<SpawnedEnemy>();
            if (enemy != null)  
            {
                Debug.Log($"[Bullet] SpawnedEnemy 컴포넌트 찾음, TakeDamage 호출 예정");
                enemy.TakeDamage(damage);
                
                // 오브젝트가 삭제되었는지 확인
                if (other.gameObject != null)
                {
                    Debug.Log($"[Bullet] 총알이 적 {enemyName}에게 {damage} 데미지를 입혔습니다.");
                }
                else
                {
                    Debug.Log($"[Bullet] 총알이 적 {enemyName}에게 {damage} 데미지를 입혀 적이 파괴되었습니다!");
                }
            }
            else
            {
                Debug.LogWarning($"[Bullet] 적 오브젝트에 SpawnedEnemy 컴포넌트가 없습니다! 오브젝트: {enemyName}");
            }
            
            // 총알 파괴
            Debug.Log($"[Bullet] 총알 파괴됨: {gameObject.name}");
            Destroy(gameObject);
        }
        // 적 타워와 충돌 처리 추가
        else if (other.gameObject.CompareTag("EnemyTower"))
        {
            string towerName = other.gameObject.name; // 삭제되기 전에 이름 저장
            Debug.Log($"[Bullet] 적 타워와 충돌! 타워 이름: {towerName}");
            
            EnemyTower enemyTower = other.gameObject.GetComponent<EnemyTower>();
            if (enemyTower != null)
            {
                Debug.Log($"[Bullet] EnemyTower 컴포넌트 찾음, TakeDamage 호출 예정");
                enemyTower.TakeDamage(damage);
                
                // 오브젝트가 삭제되었는지 확인
                if (other.gameObject != null)
                {
                    Debug.Log($"[Bullet] 총알이 적 타워 {towerName}에게 {damage} 데미지를 입혔습니다.");
                }
                else
                {
                    Debug.Log($"[Bullet] 총알이 적 타워 {towerName}에게 {damage} 데미지를 입혀 타워가 파괴되었습니다!");
                }
            }
            else
            {
                Debug.LogWarning($"[Bullet] 적 타워 오브젝트에 EnemyTower 컴포넌트가 없습니다! 오브젝트: {towerName}");
            }
            
            // 총알 파괴
            Debug.Log($"[Bullet] 총알 파괴됨: {gameObject.name}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[Bullet] Enemy나 EnemyTower 태그가 아닌 오브젝트와 충돌: {other.gameObject.tag}, 오브젝트: {other.gameObject.name}");
        }
    }

} 