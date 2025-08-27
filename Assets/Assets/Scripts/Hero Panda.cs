using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HeroPanda : MonoBehaviour
{
    [Header("플레이어 스탯")]
    public int hp;
    public int maxhp = 100;
    public float hprate;
    public float moveSpeed = 10f;
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    [Tooltip("초당 공격 횟수. 애니메이션 속도와 연관됩니다.")]
    public float attackSpeed = 1f;

    [Header("레벨 및 경험치")]
    public int level = 1;
    public float exp = 0;
    public float requetExp = 100;
    public TextMeshProUGUI levelText;
    public float exprate;

    // --- 내부 변수 ---
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D heroCollider;

    private bool isSkill2 = false;

    private bool isAttacking = false;
    public bool isDie = false;

    // 자동 공격을 위한 변수들 (InstalledUnit과 동일한 흐름)
    private float attackTimer = 0f;
    private Transform currentTarget;

    // 애니메이션 관련 변수들 (InstalledUnit과 동일)
    private Vector2 lastDirection = Vector2.down; // 기본 아래 방향
    private Vector2 lastAnimationDirection = Vector2.down; // 애니메이션에 마지막으로 적용된 방향
    private bool isAnimationUpdating = false; // 애니메이션 업데이트 중인지 체크
    private float animationTransitionDelay = 0.1f; // 애니메이션 전환 딜레이

    public bool isFollowingHero = true;

    
    public GameObject slash;
    public GameObject hpbar;
    public GameObject expbar;
    public GameObject skill2Effect; // 스킬2 이펙트 (인스펙터에서 할당)
    
    private hpbar hpBarComponent;
    private hpbar expBarComponent;

    public static HeroPanda instance;

    public InstallManager installManager;
    public GameObject loading;

    public Herostate herostate;
    public bool stop=false;
    
    [Header("부활 설정")]
    public float reviveDelay = 3f; // 부활 대기 시간
    [Range(0f,1f)] public float reviveHpRatio = 0.5f; // 부활 시 회복 비율
    public Transform respawnPoint; // 지정 시 해당 위치로 부활
    private Vector3 initialSpawnPosition; // 미지정 시 시작 위치로 부활

    public bool isSkill1OnCooldown = false;
    public bool isSkill2OnCooldown = false;
    public bool isSkill3OnCooldown = false;
    public float skill1Cooldown = 10f;
    public float skill2Cooldown = 10f;
    public float skill3Cooldown = 10f;

    void Start()
    {
        // 필수 컴포넌트들을 시작 시 한 번만 가져와 변수에 저장 (캐싱)
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        heroCollider = GetComponent<Collider2D>();
        if (hpbar != null)
        {
            hpBarComponent = hpbar.GetComponent<hpbar>();
        }
        if (expbar != null)
        {
            expBarComponent = expbar.GetComponent<hpbar>();
        }
        instance = this;

    
        hp=maxhp;
        herostate.Setstate(1,0,1);
        initialSpawnPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.instance.gameStopUI.activeSelf)
        {
            return;
        }

        if (!isDie)
        {


            if (!stop && isFollowingHero)
            {
                HandleMovementInput();
                HandleAttackInput();
                HandleSkill1Input();
                HandleSkill2Input();
                HandleSkill3Input();

            }
            else
            {
                animator.SetFloat("speed", 0);
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", 0);
                animator.SetBool("isAttacking", false);
                animator.SetBool("isSkill1", false);
                animator.SetBool("isSkill2", false);
                animator.SetBool("isSkill3", false);
                animator.SetBool("ismoving", false);
            }

            if (isSkill1OnCooldown && !IsInvoking("EndSkill1Cooldown"))
            {
                Invoke("EndSkill1Cooldown", skill1Cooldown);
            }
            if (isSkill2OnCooldown && !IsInvoking("EndSkill2Cooldown"))
            {
                Invoke("EndSkill2Cooldown", skill2Cooldown);
            }
            if (isSkill3OnCooldown && !IsInvoking("EndSkill3Cooldown"))
            {
                Invoke("EndSkill3Cooldown", skill3Cooldown);
            }
            if (isSkill1OnCooldown)
            {
                herostate.SetSkill1Cooldown();
            }
            if (isSkill2OnCooldown)
            {
                herostate.SetSkill2Cooldown();
            }
            if (isSkill3OnCooldown)
            {
                herostate.SetSkill3Cooldown();
            }
            if (!isSkill1OnCooldown)
            {
                herostate.SetSkill1CooldownEnd();
            }
            if (!isSkill2OnCooldown)
            {
                herostate.SetSkill2CooldownEnd();
            }
            if (!isSkill3OnCooldown)
            {
                herostate.SetSkill3CooldownEnd();
            }




            if (exp >= requetExp) { LevelUp(); }

            // --- 애니메이션 업데이트 ---
            UpdateAnimation(rb.linearVelocity);

            // --- 상태 체크 ---
            CheckLevelUp();
            CheckDeath();

            // --- 자동 공격 로직 ---
            AutoAttack();
            hprate = (float)hp / maxhp;
            exprate = exp / requetExp;

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

    // 스킬/행동 직전 캐릭터가 바라볼 방향을 강제로 맞추기 위한 유틸
    private void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;
        Vector2 normalized = direction.normalized;
        lastDirection = normalized;
        lastAnimationDirection = normalized;
        animator.SetFloat("MoveX", Mathf.Abs(normalized.x));
        animator.SetFloat("MoveY", normalized.y);
        if (Mathf.Abs(normalized.x) > 0.01f)
        {
            spriteRenderer.flipX = normalized.x < 0f;
        }
    }

    // 입력이 있으면 입력 방향, 없으면 마지막 애니메이션 방향을 반환
    private Vector2 GetAimDirection()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            return moveInput;
        }
        return lastAnimationDirection.sqrMagnitude > 0.0001f ? lastAnimationDirection : Vector2.down;
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
         // StartAttack();
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

    private void HandleSkill1Input()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isDie && !isSkill1OnCooldown)
        {
            StartSkill1();
        }
    }
    private void StartSkill1()
    {
        FaceDirection(GetAimDirection());
        animator.SetBool("isSkill1", true);
        isSkill1OnCooldown = true;
        PerformSkill(1);
        Invoke("EndSkill1", 1.5f);
    }
    private void EndSkill1()
    {
        
        animator.SetBool("isSkill1", false);
        animator.Rebind();
        animator.Update(0f);
    }
    private void HandleSkill3Input()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDie && !isSkill3OnCooldown)
        {
            StartSkill3();
        }
    }

    private void StartSkill3()
    {
        FaceDirection(GetAimDirection());
        animator.SetBool("isSkill3", true);
        isSkill3OnCooldown = true;
        PerformSkill(3);
        transform.DOMove(transform.position+new Vector3(moveInput.x*10,moveInput.y*10,0)*0.5f,1.5f);
        Invoke("EndSkill3", 1.5f);
        Invoke("EndSkill3Cooldown", skill3Cooldown);
    }
    private void EndSkill3()
    {
        animator.SetBool("isSkill3", false);
        animator.Rebind();
        animator.Update(0f);
        
    }

    private void HandleSkill2Input()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isDie && !isSkill2OnCooldown)
        {
            
            StartSkill2();
        }
    }

    private void StartSkill2()
    {
        FaceDirection(GetAimDirection());
        
        animator.SetBool("isSkill2", true);
        isSkill2 = true;
        
        // 스킬2 시작 시 즉시 공격
        PerformSkill(2);
        
        // 스킬2 지속 시간 동안 주기적으로 공격 (0.5초마다)
        InvokeRepeating("PerformSkill2Attack", 0.5f, 0.5f);
        
        
        // 스킬2 종료 (3초 후)
        Invoke("EndSkill2", 2f);
    }

    private void EndSkill2()
    {
        isSkill2 = false;
        animator.SetBool("isSkill2", false);
        animator.Rebind();
        animator.Update(0f);
        
       
        
        // 스킬2 이펙트 비활성화
        
        isSkill2OnCooldown = true;
        Invoke("EndSkill2Cooldown", skill2Cooldown);
        
        
    }

    private void EndSkill2Cooldown()
    {
        isSkill2OnCooldown = false;
    }
    private void EndSkill3Cooldown()
    {
        isSkill3OnCooldown = false;
    }
    private void EndSkill1Cooldown()
    {
        isSkill1OnCooldown = false;
    }

    // 스킬2 공격 수행
    private void PerformSkill2()
    {
        if (isDie || !isSkill2) return;
        
        Debug.Log("스킬2 공격 실행!");
        
        // 스킬2는 더 넓은 범위와 강한 데미지
        float skill2Range = attackRange * 1.5f; // 공격 범위
        int skill2Damage = attackDamage + 10;  
        
        // 스킬2 범위 내 모든 적 찾기
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skill2Range);
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy") || enemyCollider.CompareTag("EnemyTower"))
            {
                
                if (enemyCollider.GetComponent<SpawnedEnemy>() != null)
                {
                    enemyCollider.GetComponent<SpawnedEnemy>().TakeDamage(skill2Damage);
                }
                else if (enemyCollider.GetComponent<EnemyTower>() != null)
                {
                    enemyCollider.GetComponent<EnemyTower>().TakeDamage(skill2Damage);
                }
            }
        }
    }

    // 통합 스킬 수행 함수 (애니메이션 이벤트에서 skillId(int) 전달하여 호출 가능)
    private void PerformSkill(int skillId)
    {
        if (isDie) return;
        if (skillId == 1)
        {
            PerformSkill1();
        }
        else if (skillId == 2)
        {
            PerformSkill2();
        }
        else if (skillId == 3)
        {
            PerformSkill3();
        }
    }

    // 스킬1: 근거리 범위 공격(소형)
    private void PerformSkill1()
    {
        float skill1Range = attackRange * 1.2f;
        int skill1Damage = attackDamage + 5;
        Debug.Log("스킬1 실행!");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skill1Range);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy") || enemyCollider.CompareTag("EnemyTower"))
            {
                var enemy = enemyCollider.GetComponent<SpawnedEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skill1Damage);
                    continue;
                }
                var enemyTower = enemyCollider.GetComponent<EnemyTower>();
                if (enemyTower != null)
                {
                    enemyTower.TakeDamage(skill1Damage);
                }
            }
        }
    }

    // 스킬3: 근거리 대형 범위 공격(강력)
    private void PerformSkill3()
    {
        float skill3Range = attackRange * 2f;
        int skill3Damage = attackDamage + 20;
        Debug.Log("스킬3 실행!");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skill3Range);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy") || enemyCollider.CompareTag("EnemyTower"))
            {
                var enemy = enemyCollider.GetComponent<SpawnedEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skill3Damage);
                    continue;
                }
                var enemyTower = enemyCollider.GetComponent<EnemyTower>();
                if (enemyTower != null)
                {
                    enemyTower.TakeDamage(skill3Damage);
                }
            }
        }
    }

    // 스킬2 이펙트 활성화
    void skill2EffectActive()
    {
        if (skill2Effect != null)
        {
            skill2Effect.SetActive(true);
        }
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
        maxhp += 10;

        
         herostate.Setstate(hprate,exprate,level);
    }

    void Die()
    {
        isDie = true;
        
        
        // DOTween 애니메이션 중지
        transform.DOKill();
        
        // 스킬 상태 초기화
        isSkill2 = false;
        isSkill1OnCooldown = false;
        isSkill2OnCooldown = false;
        isSkill3OnCooldown = false;
        
        // 애니메이션 상태 머신 리셋
        animator.Rebind();
        animator.Update(0f);
        
        // 애니메이션 상태 정리
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isdie", true);
        
        // 사망 상태 설정
        herostate.Setstate(0,exprate,level);
        rb.linearVelocity = Vector2.zero;
        heroCollider.enabled = false;
        herostate.SetReviveIcon();
    }

    public void TakeDamage(int damageAmount)
    {
        hp -= damageAmount;
        
        OnMouseUp();
        if (hp < 0) { hp = 0; }
        herostate.Setstate(hprate,exprate,level);

        if (hp <= 0 && !isDie)
        {
            Die();
        }
    }

    public void PerformAttackCheck()
    {
        if (isDie&&stop) return;

        
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
            
            if(closestEnemy.GetComponent<SpawnedEnemy>()!=null){
                closestEnemy.GetComponent<SpawnedEnemy>().TakeDamage(attackDamage);
            }
            else if(closestEnemy.GetComponent<EnemyTower>()!=null){
                closestEnemy.GetComponent<EnemyTower>().TakeDamage(attackDamage);
            }
        }
    }

    // InstalledUnit과 유사한 자동 공격 루틴
    private void AutoAttack()
    {
        if (isDie) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / Mathf.Max(attackSpeed, 0.001f))
        {
            currentTarget = FindNearestTarget();

            if (currentTarget != null)
            {
                // 공격 시작 상태
                isAttacking = true;
                if (animator != null)
                {
                    animator.SetBool("isAttacking", true);
                }

                // 타겟 방향으로 바라보도록 애니메이션 방향 업데이트
                Vector2 direction = (currentTarget.position - transform.position).normalized;
                UpdateAnimation(direction);

                // 근접 공격 데미지 적용 (총알 사용 없음)
                var enemy = currentTarget.GetComponent<SpawnedEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
                else
                {
                    var enemyTower = currentTarget.GetComponent<EnemyTower>();
                    if (enemyTower != null)
                    {
                        enemyTower.TakeDamage(attackDamage);
                    }
                }

                // 슬래시 이펙트를 짧게 표시
                if (slash != null)
                {
                    StartCoroutine(ShowSlashBriefly(0.2f));
                }
            }
            else
            {
                // 타겟이 없으면 공격 상태 해제
                isAttacking = false;
                if (animator != null)
                {
                    animator.SetBool("isAttacking", false);
                }
            }

            attackTimer = 0f;
        }
    }

    private IEnumerator ShowSlashBriefly(float duration)
    {
        slash.SetActive(true);
        yield return new WaitForSeconds(duration);
        slash.SetActive(false);
    }

    // InstalledUnit.FindNearestTarget과 동일한 방식
    private Transform FindNearestTarget()
    {
        Collider2D[] allCollidersInRange = Physics2D.OverlapCircleAll(transform.position, attackRange);

        Transform nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D collider in allCollidersInRange)
        {
            if (collider.CompareTag("Enemy") || collider.CompareTag("EnemyTower"))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = collider.transform;
                }
            }
        }

        return nearestTarget;
    }

    // 공격 범위를 에디터 Scene 뷰에서 시각적으로 보여주는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    

    void slashActive(){
        slash.SetActive(true);
    }

    void slashInactive(){
        slash.SetActive(false);
    }

    void Destroy() {
        // 부활 시스템 도입으로 비활성화는 하지 않음 (호환 유지용)
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Takeexp(float exp){
        this.exp+=exp;
        Debug.Log("exp: "+this.exp);
        herostate.Setexp(exprate);
       
    }

    public void OnMouseDown()

    {
        if(rb.linearVelocity.magnitude>0.1f)
        {
            return;
        }
        stop=true;
        rb.linearVelocity=Vector2.zero;
        if (loading == null)
        {
            Debug.LogWarning("loading UI가 할당되지 않았습니다.");
            return;
        }
        loading.SetActive(true);
        var loadingComponent = loading.GetComponent<loading>();
        if (loadingComponent != null)
        {
            loadingComponent.green();
            loadingComponent.StartInstall();
        }
    }
    public void OnMouseUp()
    {
        stop=false;
        var loadingComponent = loading != null ? loading.GetComponent<loading>() : null;
       
        if (loading != null)
        {
            loadingComponent.green();
            loading.SetActive(false);
            
        }
    }
    public void heroActive(){
        gameObject.SetActive(true);
    }

    // --- 부활 루틴 ---
    private IEnumerator HandleDeathAndRevive()
    {
        // 사망 연출 대기
        yield return new WaitForSeconds(1f);

        // 사망 상태에서 잠시 숨김/충돌 비활성화
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (heroCollider != null) heroCollider.enabled = false;

       if(isDie){
        StartCoroutine(HandleDeathAndRevive());
       }
        Revive();
    }

    public void Revive()
    {
        if(!isDie) return;
        if(GameManager.instance.GetGold()<80)
        {
            return;
        }
        herostate.SetNormalIcon();
        GameManager.instance.SpendGold(80);

        // 체력 복구 (레벨/경험치/스탯 유지)
        heroCollider.enabled = true;
        hp = maxhp;
        isDie = false;
        animator.Rebind();
        animator.SetBool("isdie", false);

       
        if (heroCollider != null) heroCollider.enabled = true;

        // UI 갱신
        hprate = (float)hp / maxhp;
        herostate.Setstate(hprate, exprate, level);
    }

    // 외부에서 즉시 부활 시키고 싶을 때 호출
    public void ReviveNow(bool fullHeal = false)
    {
        if (!isDie)
        {
            return;
        }
        StopAllCoroutines();
        Revive();
        if (fullHeal)
        {
            hp = maxhp;
            hprate = (float)hp / Mathf.Max(1, maxhp);
            exprate = requetExp > 0 ? exp / requetExp : 0f;
            herostate.Setstate(hprate, exprate, level);
        }
    }

    public void isFollowingHerochange()
    {
        isFollowingHero = !isFollowingHero;
        rb.linearVelocity = Vector2.zero;
    }
}

