using UnityEngine;
using DG.Tweening; 
using System.Collections.Generic;
using System.Collections;

public class HeroPanda : MonoBehaviour
{
    [Header("플레이어 스탯")]
    public int hp = 100;
    public float moveSpeed = 10f;
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    [Tooltip("초당 공격 횟수. 애니메이션 속도와 연관됩니다.")]
    public float attackSpeed = 1f;

    [Header("레벨 및 경험치")]
    public int level = 1;
    public float exp = 0;
    public float requetExp = 100;

    // --- 내부 변수 ---
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isAttacking = false;
    private bool isDie = false;

    // 애니메이션 관련 변수들 (InstalledUnit과 동일)
    private Vector2 lastDirection = Vector2.down; // 기본 아래 방향
    private Vector2 lastAnimationDirection = Vector2.down; // 애니메이션에 마지막으로 적용된 방향
    private bool isAnimationUpdating = false; // 애니메이션 업데이트 중인지 체크
    private float animationTransitionDelay = 0.1f; // 애니메이션 전환 딜레이

    private List<Collider2D> targetEnemies = new List<Collider2D>();
    private float clearTime = 0;

    public GameObject slash;
    public GameObject hpbar;

    void Start()
    {
        // 필수 컴포넌트들을 시작 시 한 번만 가져와 변수에 저장 (캐싱)
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- 입력 처리 ---
        HandleMovementInput();
        HandleAttackInput();

        // --- 애니메이션 업데이트 ---
        UpdateAnimation(rb.linearVelocity);

        // --- 상태 체크 ---
        CheckLevelUp();
        CheckDeath();
        clearTime=Time.time;
        if(clearTime>5){
            clearTime=0;
            targetEnemies.Clear();
        }
        if(targetEnemies.Count>0){
            expcheck();
        }

    }

    void FixedUpdate()
    {
        // 물리 기반 이동은 FixedUpdate에서 처리
        Move();
    }

    // --- 애니메이션 업데이트 (InstalledUnit과 동일한 로직) ---
    private void UpdateAnimation(Vector2 velocity)
    {
        if (animator == null) return;

        // 이동 중일 때만 방향 업데이트
        if (velocity.sqrMagnitude > 0.01f)
        {
            lastDirection = velocity.normalized;
            animator.SetFloat("speed", velocity.magnitude);
        }
        else
        {
            // 이동이 멈췄을 때 Idle 애니메이션 재생
            if (!isAttacking)
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", 0f);
            }
        }
        animator.SetFloat("speed", velocity.magnitude);

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

        // spriteRenderer.flipX를 사용하여 좌우 반전
        if (Mathf.Abs(newDirection.x) > 0.01f)
            spriteRenderer.flipX = newDirection.x < 0;

        isAnimationUpdating = false;
    }

    // --- 입력 처리 함수들 ---
    private void HandleMovementInput()
    {
        if (isDie) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        // 이동 입력이 있을 때만 방향 전환
        if (moveX != 0)
        {
            // spriteRenderer.flipX를 사용하여 좌우 반전 (transform.localScale 대신)
            spriteRenderer.flipX = moveX < 0;
        }
    }

    private void HandleAttackInput()
    {
        // 스페이스바를 누르면 공격 시작
        if (Input.GetKeyDown(KeyCode.Space) && !isDie && !isAttacking)
        {
            StartAttack();
        }
    }

    // 공격 시작
    private void StartAttack()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);
        
        Invoke("slashActive", 0.5f);
        PerformAttackCheck();
        Invoke("slashInactive", 2f);
        Invoke("EndAttack", 0.5f);
    }

    // 공격 종료
    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    // --- 상태 및 로직 함수들 ---
    private void Move()
    {
        if (!isDie)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CheckLevelUp()
    {
        if (exp >= requetExp)
        {
            LevelUp();
        }
    }
    
    private void CheckDeath()
    {
        if (hp <= 0 && !isDie)
        {
            Die();
        }
    }

    void LevelUp()
    {
        level++;
        exp -= requetExp; // 남은 경험치 이월 (exp = 0; 보다 좋은 방식)
        requetExp *= 1.5f;

        // 스탯 성장
        moveSpeed += 0.5f;
        attackRange += 0.1f;
        attackDamage += 2;
        hp += 10;

        Debug.Log($"LEVEL UP! 현재 레벨: {level}");
        // 여기에 레벨업 이펙트나 사운드 추가
    }

    void Die()
    {
        isDie = true;
        animator.SetBool("isdie", true);
        Invoke("De",1f);
        Debug.Log("Hero is dead");
        // 여기에 죽음 애니메이션, 게임 오버 처리 등 추가
    }

   
    public void PerformAttackCheck()
    {
        if (isDie) return;
        
        Debug.Log("Animation Event: Attack Check!");

        // 1. 공격 범위 내 모든 적 찾기
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // 2. 가장 가까운 적 찾기
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy")||enemyCollider.CompareTag("EnemyTower"))
            {
                float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemyCollider.transform;
                }
            }
        }

        // 3. 가장 가까운 적에게 대미지 주기
        if (closestEnemy != null)
        {
            Debug.Log($"가장 가까운 적 [{closestEnemy.name}]을(를) 공격!");
            // Enemy 스크립트의 TakeDamage 함수를 호출
            targetEnemies.Add(closestEnemy.GetComponent<Collider2D>());
            if(closestEnemy.GetComponent<SpawnedEnemy>()!=null){
                closestEnemy.GetComponent<SpawnedEnemy>().TakeDamage(attackDamage);
            }
            else if(closestEnemy.GetComponent<EnemyTower>()!=null){
                closestEnemy.GetComponent<EnemyTower>().TakeDamage(attackDamage);
            }
        }
    }

    // 공격 범위를 에디터 Scene 뷰에서 시각적으로 보여주는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    void expcheck(){
        foreach(Collider2D enemy in targetEnemies){
            if(enemy==null){
                targetEnemies.Remove(enemy);
                exp+=10;
            }
            
        }
    }
    void slashActive(){
        slash.SetActive(true);
    }
    void slashInactive(){
        slash.SetActive(false);
    }

    void De() {
        Destroy(gameObject);
    }
}   

