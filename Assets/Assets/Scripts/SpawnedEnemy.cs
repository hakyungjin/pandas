using UnityEngine;
using UnityEngine.UI;

public class SpawnedEnemy : MonoBehaviour
{
    public Enemy enemyData;
    
    private hpbar hpBarComponent;
    private Transform hpBarTransform;
    private float currentHp;
    public float moveSpeed = 1.5f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    private float lastAttackTime = 0f;
    private Transform currentTarget;

    // 애니메이션 관련 변수들
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down; // 기본 아래 방향
    private Vector2 lastAnimationDirection = Vector2.down; // 애니메이션에 마지막으로 적용된 방향
    private bool isAnimationUpdating = false; // 애니메이션 업데이트 중인지 체크
    private float animationTransitionDelay = 0.1f; // 애니메이션 전환 딜레이
    private bool isAttacking = false; // 공격 중인지 체크

    void Start()
    {
        if (enemyData != null)
        {
            InitializeEnemy();
        }
    }

    void InitializeEnemy()
    {
        currentHp = enemyData.hp;

        // 스프라이트 렌더러 설정
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = enemyData.unitSprite;
        spriteRenderer.color = enemyData.unitColor;

        // BoxCollider2D 추가 (충돌 감지용)
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 스프라이트 크기에 맞게 콜라이더 크기 자동 조정
        if (enemyData.unitSprite != null)
        {
            boxCollider.size = enemyData.unitSprite.bounds.size;
        }

        // Rigidbody2D 추가 (물리 시뮬레이션용)
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        // Rigidbody2D 설정
        rigidbody.gravityScale = 0f; // 중력 비활성화
        rigidbody.linearDamping = 1f; // 저항 설정
        rigidbody.angularDamping = 0.05f; // 각도 저항 설정
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정

        // Enemy 태그 설정
        gameObject.tag = "Enemy";
        
        // 애니메이션 컴포넌트 가져오기
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = enemyData.animatorController;
        
        this.spriteRenderer = spriteRenderer;

        // HP바 생성 (Enemy 데이터에서 가져오기)
        if (enemyData.hpBarPrefab != null)
        {
            GameObject hpBarInstance = Instantiate(enemyData.hpBarPrefab, transform);
            hpBarInstance.transform.localPosition = new Vector3(0, 0.55f, 0);

            hpBarTransform = hpBarInstance.transform;
            hpBarComponent = hpBarInstance.GetComponentInChildren<hpbar>();

            // 캔버스를 월드 스페이스로 설정
            Canvas canvas = hpBarInstance.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = 6;
            }

            // HP바 초기화
            if (hpBarComponent != null)
            {
                hpBarComponent.maxHp = enemyData.hp;
                hpBarComponent.SetHp(currentHp);
            }
        }
        IgnoreUnitCollisions();
    }

    public void TakeDamage(int damageAmount)
    {
        Debug.Log($"[SpawnedEnemy] {gameObject.name}이(가) {damageAmount} 데미지를 받았습니다. 현재 HP: {currentHp} -> {currentHp - damageAmount}");
        
        if (hpBarComponent != null)
        {
            hpBarComponent.decreaseHp(damageAmount);
            currentHp = hpBarComponent.currentHp; // HP바 컴포넌트의 currentHp와 동기화
        }
        else
        {
            currentHp -= damageAmount;
            if (currentHp < 0) currentHp = 0;
        }

        if (currentHp <= 0)
        {
            Debug.Log($"[SpawnedEnemy] {gameObject.name}이(가) 파괴되었습니다!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[SpawnedEnemy] {gameObject.name}의 남은 HP: {currentHp}");
        }
    }

    void Update()
    {
        if (hpBarTransform != null)
        {
            hpBarTransform.rotation = Camera.main.transform.rotation;
        }

        if (currentTarget == null)
        {
            FindTarget();
        }
        else
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);

            // 타겟이 너무 멀어졌으면 다시 탐색
            if (distance > detectionRange * 1.5f)
            {
                currentTarget = null;
                return;
            }

            // 공격 사거리 이내면 공격
            if (distance <= attackRange)
            {
                TryAttack();
                Debug.Log($"[SpawnedEnemy] {gameObject.name}이(가) {currentTarget.name}을(를) 공격했습니다.");
            }
            else
            {
                // 공격 중이 아니면 이동
                if (!isAttacking)
                {
                    // 타겟 방향으로 이동
                    Vector2 direction = (currentTarget.position - transform.position).normalized;
                    transform.Translate(direction * moveSpeed * Time.deltaTime);
                    
                    // 애니메이션 업데이트
                    UpdateAnimation(direction * moveSpeed);
                }
            }
        }
    }

    void UpdateAnimation(Vector2 velocity)
    {
        // 이동 중일 때만 방향 업데이트
        if (velocity.sqrMagnitude > 0.01f)
        {
            lastDirection = velocity.normalized;
        }

        // 방향이 실제로 변경되었을 때만 애니메이션 업데이트 (애니메이션 업데이트 중이 아닐 때)
        float directionThreshold = 0.3f; // 방향 변화 임계값
        if (Vector2.Distance(lastDirection, lastAnimationDirection) > directionThreshold && !isAnimationUpdating)
        {
            StartCoroutine(SmoothAnimationTransition(lastDirection));
        }
    }

    System.Collections.IEnumerator SmoothAnimationTransition(Vector2 newDirection)
    {
        isAnimationUpdating = true;
        
        // 딜레이 후 애니메이션 업데이트
        yield return new WaitForSeconds(animationTransitionDelay);
        
        lastAnimationDirection = newDirection;
        
        if (animator != null)
        {
            animator.SetFloat("MoveX", newDirection.x > 0 ? newDirection.x : -newDirection.x); // 항상 양수로
            animator.SetFloat("MoveY", newDirection.y);
        }

        if (Mathf.Abs(newDirection.x) > 0.01f && spriteRenderer != null)
            spriteRenderer.flipX = newDirection.x < 0;
            
        isAnimationUpdating = false;
    }

    void FindTarget()
    {
        // "Tower"와 "Unit" 태그를 가진 오브젝트 탐색
        GameObject[] towerTargets = GameObject.FindGameObjectsWithTag("Tower");
        GameObject[] unitTargets = GameObject.FindGameObjectsWithTag("Unit");
        
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        // Tower 태그 오브젝트 탐색
        foreach (GameObject target in towerTargets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < detectionRange && distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = target.transform;
            }
        }

        // Unit 태그 오브젝트 탐색
        foreach (GameObject target in unitTargets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < detectionRange && distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = target.transform;
            }
        }

        currentTarget = nearestTarget;
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        
        // 공격 애니메이션 시작
        isAttacking = true;
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
        }
        
        // 타겟의 태그에 따라 다른 공격 로직 실행
        if (currentTarget.CompareTag("Tower"))
        {
            // 타워 공격: InstallZone의 DecreaseHp 메서드 호출
            InstallZone installZone = currentTarget.GetComponent<InstallZone>();
            if (installZone != null)
            {
                installZone.DecreaseHp(attackDamage);
                Debug.Log($"[SpawnedEnemy] 타워 {currentTarget.name}에 {attackDamage} 데미지를 가했습니다.");
            }
        }
        else if (currentTarget.CompareTag("Unit"))
        {
            // 유닛 공격: InstalledUnit의 TakeDamage 메서드 호출
            InstalledUnit installedUnit = currentTarget.GetComponent<InstalledUnit>();
            if (installedUnit != null)
            {
                installedUnit.TakeDamage(attackDamage);
                Debug.Log($"[SpawnedEnemy] 유닛 {currentTarget.name}에 {attackDamage} 데미지를 가했습니다.");
            }
        }
        
        // 공격 애니메이션 종료 (공격 쿨다운 후)
        StartCoroutine(EndAttackAnimation());
    }
    
    System.Collections.IEnumerator EndAttackAnimation()
    {
        // 공격 애니메이션 지속 시간 (공격 쿨다운과 동일하게 설정)
        yield return new WaitForSeconds(attackCooldown);
        
        // 공격 애니메이션 종료
        isAttacking = false;
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
        }
    }
     void IgnoreUnitCollisions()
    {
        // 모든 Unit 태그 오브젝트와의 충돌 무시
        GameObject[] units = GameObject.FindGameObjectsWithTag("Enemy");
        Collider2D myCollider = GetComponent<Collider2D>();
        
        foreach (GameObject unit in units)  
        {
            if (unit != gameObject) // 자기 자신 제외
            {
                Collider2D otherCollider = unit.GetComponent<Collider2D>();
                if (otherCollider != null && myCollider != null)
                {
                    Physics2D.IgnoreCollision(myCollider, otherCollider, true);
                }
            }
        }
    }
}
