using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InstalledUnit : MonoBehaviour
{
    public Tower unitData;
    public int currentHealth;

    [Header("HP Bar Prefab")]
    public GameObject hpBarPrefab;

    [Header("공격 관련")]
    // Enemy 태그를 사용하므로 enemyLayer는 더 이상 필요하지 않음
    
    [Header("이동 관련")]
    public float moveSpeed = 2f; // 이동 속도

    private hpbar hpBarComponent;
    private Transform hpBarTransform;
    private float attackTimer;
    private Transform currentTarget;
    private LineRenderer rangeRenderer;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down; // 기본 아래 방향
    private Vector2 lastAnimationDirection = Vector2.down; // 애니메이션에 마지막으로 적용된 방향
    private bool isAnimationUpdating = false; // 애니메이션 업데이트 중인지 체크
    private float animationTransitionDelay = 0.1f; // 애니메이션 전환 딜레이

    private bool isAttacking = false;


    
    
    private bool isPreview = false; // 프리뷰 모드 여부
    private bool isMovingByCommand = false; // moveUnit 명령으로 이동 중인지 여부

    private Rigidbody2D rigidbody;
    
    private Vector3 moveTargetPosition;
    

    private List<Exhence> exhence = new List<Exhence>();

    private int attack;
    
    private float attackSpeed;

    public void Initialize(Tower data, bool preview = false)
    {
        unitData = data;
       
        isPreview = preview;
        rigidbody = GetComponent<Rigidbody2D>();

        currentHealth = unitData.hp;
         attack = unitData.attack;
         moveSpeed = unitData.moveSpeed;
         attackSpeed = unitData.attackSpeed;
         animator = GetComponent<Animator>();
         spriteRenderer = GetComponent<SpriteRenderer>();
        

        // 유닛별 애니메이션 컨트롤러 설정
        if (animator != null && unitData.animatorController != null)
        {
            animator.runtimeAnimatorController = unitData.animatorController;
            Debug.Log($"애니메이션 컨트롤러 설정: {unitData.animatorController.name}");
        }



        // Create HP Bar (프리뷰가 아닐 때만)
        if (!isPreview)
        {
            GameObject hpBarInstance = Instantiate(hpBarPrefab, transform);
            hpBarInstance.transform.localPosition = new Vector3(0, 0.6f, 0); // Position above the unit

            hpBarTransform = hpBarInstance.transform;
            hpBarComponent = hpBarInstance.GetComponent<hpbar>();

            // Force Canvas to World Space
            Canvas canvas = hpBarInstance.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main; // Link camera (optional)
                canvas.sortingOrder = 6;
            }

            if (hpBarComponent != null)
            {
                hpBarComponent.maxHp = unitData.hp;
                hpBarComponent.SetHp(currentHealth);
            }

            // 공격 범위 표시용 LineRenderer 생성 (프리뷰가 아닐 때만)
            CreateRangeRenderer();
        }

        // 공격 타이머 초기화
        attackTimer = 0f;
        
        // 이동을 위한 Rigidbody2D 설정 (프리뷰가 아닐 때만)
        if (!isPreview)
        {
            SetupMovementComponents();
        }
    }

    void SetupMovementComponents()
    {
        // Unit 태그 설정
        if (gameObject.tag != "Unit")
        {
            gameObject.tag = "Unit";
        }
        
        // 같은 유닛끼리 충돌 무시
        IgnoreUnitCollisions();
    }
    
    void IgnoreUnitCollisions()
    {
        // 모든 Unit 태그 오브젝트와의 충돌 무시
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
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

    void CreateRangeRenderer()
    {
        // LineRenderer 컴포넌트 추가
        rangeRenderer = gameObject.AddComponent<LineRenderer>();
        rangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        rangeRenderer.startColor = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색
        rangeRenderer.endColor = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색
        rangeRenderer.startWidth = 0.1f;
        rangeRenderer.endWidth = 0.1f;
        rangeRenderer.positionCount = 32; // 원을 그리기 위한 점 개수
        rangeRenderer.sortingOrder = 1;

        // 원 모양으로 범위 그리기
        DrawRangeCircle();
    }

    void DrawRangeCircle()
    {
        if (rangeRenderer != null && unitData != null)
        {
            float radius = unitData.attackRange;
            Vector3 center = transform.position;
            
            for (int i = 0; i < rangeRenderer.positionCount; i++)
            {
                float angle = i * 2f * Mathf.PI / rangeRenderer.positionCount;
                float x = center.x + radius * Mathf.Cos(angle);
                float y = center.y + radius * Mathf.Sin(angle);
                rangeRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (hpBarComponent != null)
        {
            hpBarComponent.SetHp(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 프리뷰 모드가 아닐 때만 실행
        if (isPreview) return;

        


        // Make the HP bar always face the camera (optional)
        if (hpBarTransform != null)
        {
            hpBarTransform.rotation = Camera.main.transform.rotation;
        }

        // 공격 범위 원 업데이트 (타워가 이동할 경우를 대비)
        if (rangeRenderer != null)
        {
            DrawRangeCircle();
        }

        // moveUnit 명령으로 이동 중인지 체크
        if (isMovingByCommand)
        {
            CheckMoveCompletion();
        }

        // 공격 로직
        if (unitData != null)
        {
            AttackLogic();
        }
        UpdateAnimation(rigidbody.linearVelocity);

    }

    void AttackLogic()
    {
        // moveUnit 명령으로 이동 중일 때는 타겟을 찾지 않음
        if (isMovingByCommand)
        {
            return;
        }

        // 공격 타이머 업데이트
        attackTimer += Time.deltaTime;

        // 공격 속도에 따라 공격 가능한지 확인
        if (attackTimer >= 1f / attackSpeed)
        {
            
            // 범위 내 적 탐지
            currentTarget = FindNearestTarget();
            Debug.Log($"[InstalledUnit] 탐지된 적: {(currentTarget != null ? currentTarget.name : "없음")}");

            if (currentTarget != null)
            {
                // 공격 중일 때는 이동 중지
                StopMovement();
                
                // 적을 향해 총알 발사
                
                attackstart();
                
                attackTimer = 0f; // 타이머 리셋
            }
            else
            {
                Debug.Log($"[InstalledUnit] 공격 범위 내에 적이 없습니다. 범위: {unitData.attackRange}");
                isAttacking = false;
                animator.SetBool("isAttacking", isAttacking);
                // 공격 범위에 적이 없으면 이동 로직 실행
                MoveToTarget();
            }
        }
    }

    Transform FindNearestTarget()
    {
        // 범위 내의 모든 콜라이더 찾기 (레이어 제한 없이)
        Collider2D[] allCollidersInRange = Physics2D.OverlapCircleAll(transform.position, unitData.attackRange);
        Debug.Log($"[InstalledUnit] 범위 내 콜라이더 수: {allCollidersInRange.Length}");
        
        Transform nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D collider in allCollidersInRange)
        {
            // Enemy 태그 또는 EnemyTower 태그를 가진 오브젝트 필터링
            if (collider.CompareTag("Enemy") || collider.CompareTag("EnemyTower"))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                Debug.Log($"[InstalledUnit] 적 발견: {collider.name}, 태그: {collider.tag}, 거리: {distance}");
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = collider.transform;
                }
            }
        }

        return nearestTarget;
    }

    void attackstart()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", isAttacking);
        UpdateAnimation(Vector2.zero);
        if(unitData.towerType==TowerType.Sniper)
        {
            FireBullet();
        }
        else if(unitData.towerType==TowerType.Normal)
        {
            normalattack();
        }
    }

    void normalattack()
    {
        Debug.Log($"[InstalledUnit] normalattack 호출됨");
        if(currentTarget!=null)
        {
            // 적인지 확인
            SpawnedEnemy enemy = currentTarget.GetComponent<SpawnedEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attack);
                Debug.Log($"[InstalledUnit] 적에게 {attack} 데미지");
            }
            else
            {
                // 적 타워인지 확인
                if (currentTarget.CompareTag("EnemyTower"))
                {
                    
                    EnemyTower enemyTower = currentTarget.GetComponent<EnemyTower>();
                     if (enemyTower != null)
                    {
                        enemyTower.TakeDamage(attack);
                    }
                    Debug.Log($"[InstalledUnit] 적 타워 {currentTarget.name}를 공격했습니다!");
                }
            }
        }
    }

    void FireBullet()
    {
        Debug.Log($"[InstalledUnit] FireBullet 호출됨 - bulletPrefab: {(unitData.bulletPrefab != null ? "있음" : "없음")}, currentTarget: {(currentTarget != null ? "있음" : "없음")}");
        
        if (unitData.bulletPrefab != null && currentTarget != null)
        {
            // 타워 정중앙에서 총알 생성
            Vector3 firePosition = transform.position;
            GameObject bulletObj = Instantiate(unitData.bulletPrefab, firePosition, Quaternion.identity);
            Debug.Log($"[InstalledUnit] 총알 오브젝트 생성됨: {bulletObj.name} at {firePosition}");
            
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            Debug.Log($"[InstalledUnit] Bullet 컴포넌트: {(bullet != null ? "찾음" : "없음")}");
            
            if (bullet != null)
            {
                // 적을 향하는 방향 계산
                Vector3 direction = (currentTarget.position - firePosition).normalized;
                
                // 총알 초기화 (방향, 속도, 데미지)
                bullet.Initialize(direction, unitData.bulletSpeed, attack);
                
                Debug.Log($"[InstalledUnit] 타워가 적을 향해 총알을 발사했습니다. 데미지: {unitData.attack}, 속도: {unitData.bulletSpeed}, 방향: {direction}");
            }
            else
            {
                Debug.LogError($"[InstalledUnit] 총알 오브젝트에 Bullet 컴포넌트가 없습니다!");
            }
        }
        else
        {
            if (unitData.bulletPrefab == null)
                Debug.LogWarning($"[InstalledUnit] bulletPrefab이 설정되지 않았습니다!");
            if (currentTarget == null)
                Debug.LogWarning($"[InstalledUnit] 공격할 적이 없습니다!");
        }
    }

    // 이동 로직 - 적이나 적 기지를 향해 이동
    void MoveToTarget()
    {
        Transform moveTarget = FindMoveTarget();
        
        
        if (moveTarget != null && rigidbody != null)
        {
            // 목표 위치로 이동 (Rigidbody2D 사용으로 부드러운 물리 이동)
            Vector2 direction = (moveTarget.position - transform.position).normalized;
            rigidbody.linearVelocity = direction * moveSpeed;
            UpdateAnimation(rigidbody.linearVelocity);
            
            Debug.Log($"[InstalledUnit] {moveTarget.name}을(를) 향해 이동 중");
        }
        else
        {
            // 목표가 없으면 이동 중지
            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector2.zero;
                UpdateAnimation(Vector2.zero);
            }
            Debug.Log($"[InstalledUnit] 이동할 목표를 찾을 수 없습니다.");
        }
    }
    




    public void moveUnit(Vector3 target)
    {
        moveTargetPosition = target;
        isMovingByCommand = true;
        while(Vector2.Distance(transform.position,target)>0.05f)
        {
            rigidbody.linearVelocity = (target - transform.position).normalized * moveSpeed;
            
        }
        rigidbody.linearVelocity = Vector2.zero;
        isMovingByCommand = false;
        
        Debug.Log($"[InstalledUnit] moveUnit 시작 - 목표 위치: {target}, 타겟 탐지 중단");
    }

    void CheckMoveCompletion()
{
    float distance = Vector2.Distance(transform.position, moveTargetPosition);
    if (distance <= 0.05f) // 너무 작은 값이면 멈추지 않음
    {
        isMovingByCommand = false;
        StopMovement();
    }
}


    // 이동 중지
    void StopMovement()
    {
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector2.zero;
        }
        // moveUnit 명령에 의한 이동도 중단
        isMovingByCommand = false;
    }
    
    // 이동할 목표 찾기 (적 우선, 없으면 적 기지)
    Transform FindMoveTarget()
    {
        // 1. 가장 가까운 적 찾기 (공격 범위와 관계없이)
        SpawnedEnemy nearestEnemy = FindNearestEnemyAnywhere();
        if (nearestEnemy != null)
        {
            return nearestEnemy.transform;
        }
        
        // 2. 적이 없으면 가장 가까운 적 기지(Tower) 찾기
        Transform nearestEnemyBase = FindNearestEnemyBase();
        return nearestEnemyBase;
    }
    
    // 공격 범위와 관계없이 가장 가까운 적 찾기
    SpawnedEnemy FindNearestEnemyAnywhere()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        SpawnedEnemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject enemyObj in enemies)
        {
            SpawnedEnemy enemy = enemyObj.GetComponent<SpawnedEnemy>();
            if (enemy != null)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }
        
        return nearestEnemy;
    }
    
    // 가장 가까운 적 기지(Tower 태그) 찾기
    Transform FindNearestEnemyBase()
    {
        GameObject[] enemyBases = GameObject.FindGameObjectsWithTag("EnemyTower");
        Transform nearestBase = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject baseObj in enemyBases)
        {
            float distance = Vector2.Distance(transform.position, baseObj.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestBase = baseObj.transform;
            }
        }
        
        return nearestBase;
    }

    // 공격 범위를 시각적으로 표시
    void OnDrawGizmos()
    {
        if (unitData != null && !isPreview)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, unitData.attackRange);
        }
    }
    void Levleup(Exhence e) {
        exhence.Add(e);
        currentHealth = unitData.hp + exhence.Sum(x => x.hpBonus);
        attack = unitData.attack + exhence.Sum(x => x.damageBonus);
        moveSpeed = unitData.moveSpeed + exhence.Sum(x => x.moveSpeedBonus);
        attackSpeed = unitData.attackSpeed + exhence.Sum(x => x.attackSpeedBonus);
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

IEnumerator SmoothAnimationTransition(Vector2 newDirection)
{
    isAnimationUpdating = true;
    
    // 딜레이 후 애니메이션 업데이트
    yield return new WaitForSeconds(animationTransitionDelay);
    
    lastAnimationDirection = newDirection;
    
    animator.SetFloat("MoveX", newDirection.x > 0 ? newDirection.x : -newDirection.x); // 항상 양수로
    animator.SetFloat("MoveY", newDirection.y);

    if (Mathf.Abs(newDirection.x) > 0.01f)
        spriteRenderer.flipX = newDirection.x < 0;
        
    isAnimationUpdating = false;
}






void UpdateFlip(Vector2 direction)
{
    if (direction.x > 0)
    {
        spriteRenderer.flipX = false;
    }
    else if (direction.x < 0)
    {
        spriteRenderer.flipX = true;
    }
}




}

    
      