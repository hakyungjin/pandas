using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IDroppable
{
    void OnDropFromUI(Uiunit uiUnit);
}

public class InstallZone : MonoBehaviour, IDroppable
{
    [Header("드롭 감지 설정")]
    public bool useRaycastDrop = true; // 레이캐스트 방식 사용 여부
    [Header("설치 가능 여부")]
    public bool isInstallable = true;

    [Header("설치된 유닛 정보")]
    public Tower installedUnit; // 설치된 유닛의 ScriptableObject 정보
    public GameObject installedObject; // 인스턴스된 유닛 프리팹

    [Header("유닛 생성 설정")]
    public float spawnCooldown = 2f; // 유닛 생성 쿨타임 (초)

    private GameObject unitPrefab; // 생성할 유닛 프리팹
    private bool isSpawning = false; // 현재 생성 중인지 여부
    private float currentCooldown = 0f; // 현재 쿨타임 진행 상황

    public hpbar hpBar;
    public GameObject hpBar2; // 쿨타임바로 사용

    public hpbar hpBarInstance2;

    public GameObject TowerPrefab;

    public GameObject TowerPrefab2;
    // 캐릭터창에서 사용할 속성들
    public Sprite unitSprite;
    public Sprite iconSprite;
    public int hp;
    public int attack;
    public float attackRange;
    public float attackSpeed;
    public int level = 1; // 현재 레벨
    public GameObject levelupUI;

    public List<Exhence> exhenceList = new List<Exhence>();

    



    public Tower towerData;

    private bool isGoldTower = false;
    public int additionalGold = 1;
    

    void Start()
    {
        GameManager.instance.installZones.Add(this);

    }

    void Update()
    {
        // 오브젝트가 파괴되었는지 확인
        if (this == null) return;
        
        // 쿨타임바 업데이트
        if (isSpawning && hpBarInstance2 != null)
        {
            currentCooldown += Time.deltaTime;
            // 쿨타임 진행률을 0~1 사이로 정규화하여 HP바에 표시

            hpBarInstance2.SetHp(currentCooldown);

            // 쿨타임이 완료되면 다음 생성 사이클로
            if (currentCooldown >= spawnCooldown)
            {
                currentCooldown = 0f;
                hpBarInstance2.SetHp(0f);
            }
        }
    }



    public void OnDropFromUI(Uiunit uiUnit)
    {
        Debug.Log("InstallZone: OnDropFromUI 호출됨");
        ProcessDrop(uiUnit);
    }



    // 유닛 설치 함수 (Tower 데이터를 직접 받는 버전) - 무한 생성 시작
    public void InstallUnit(Tower towerData, GameObject unitPrefab)
    {
        // null 체크 추가
        if (towerData == null)
        {
            Debug.LogError("InstallUnit: towerData가 null입니다.");
            return;
        }
        
        if (unitPrefab == null)
        {
            Debug.LogError("InstallUnit: unitPrefab이 null입니다.");
            return;
        }
        
        if (!isInstallable) return;
        
        // 골드 체크 추가
        if (GameManager.instance != null && GameManager.instance.GetCurrentGold() < towerData.goldCost)
        {
            Debug.Log($"골드 부족: 현재 {GameManager.instance.GetCurrentGold()}, 필요 {towerData.goldCost}");
            return;
        }

        spawnCooldown = towerData.spawnCooldown;

        TowerPrefab2 = Instantiate(TowerPrefab, transform.position, Quaternion.identity);
        TowerPrefab2.transform.localPosition += new Vector3(-0.2f, 0.7f, 0);
        TowerPrefab2.GetComponent<SpriteRenderer>().sprite = towerData.TowerSprite;//타워 생성성
        attackSpeed = towerData.attackSpeed;//공격속도
        hp = towerData.hp;
        attack = towerData.attack;
        unitSprite = towerData.unitSprite;
        level = 1;
        GameManager.instance.SpendGold(towerData.goldCost);
        this.towerData = towerData;
        isGoldTower = towerData.isGoldTower;
        iconSprite=towerData.iconSprite;



        Debug.Log($"InstallUnit: {towerData.name} 설치 - 무한 생성 시작");

        // 유닛 데이터 저장
        installedUnit = towerData;
        // 업그레이드 데이터 로드 (타워의 업그레이드 테이블을 설치존에 복사)
        if (towerData.exhenceList != null)
        {
            exhenceList = new List<Exhence>();
        }
       
        this.unitPrefab = unitPrefab;
        isInstallable = false;

        GameObject hpBarInstance = Instantiate(hpBar2, transform);
        hpBarInstance.transform.localPosition = new Vector3(0, -1.7f, 0);


        hpBarInstance2 = hpBarInstance.transform.GetChild(0).GetComponent<hpbar>();

        // 캔버스를 월드 스페이스로 설정
        Canvas canvas = hpBarInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 6;
        }

        // HP바 초기화
        if (hpBarInstance2 != null)
        {
            hpBarInstance2.maxHp = spawnCooldown;
            hpBarInstance2.SetHp(0f);
            Debug.Log($"쿨타임바 초기화: maxHp={spawnCooldown}, 현재값=0");
        }



        // 생성 프로세스 시작
        if (!isSpawning)
        {
            if (!isGoldTower)
            {
                StartCoroutine(SpawnUnitsWithCooldown());

            }else{
                GameManager.instance.GoldIncrease(additionalGold);
            }
            
            
          
        }
    }

    // 쿨타임을 가지고 유닛을 무한 생성하는 코루틴
    private IEnumerator SpawnUnitsWithCooldown()
    {
        isSpawning = true;

        while (installedUnit != null && unitPrefab != null)
        {
            // 실제 유닛 생성
            CreateUnit(installedUnit, unitPrefab);

            Debug.Log($"유닛 생성 완료: {installedUnit.name}. 다음 생성까지 {spawnCooldown}초 대기");

            // 쿨타임 초기화 및 대기
            currentCooldown = 0f;
            if (hpBarInstance2 != null)
            {
                hpBarInstance2.SetHp(0);
            }

            // 쿨타임 대기 (Update에서 쿨타임바가 업데이트됨)
            yield return new WaitForSeconds(spawnCooldown);
        }

        isSpawning = false;
        Debug.Log("유닛 생성 코루틴 종료");
    }

    // 실제 유닛을 생성하는 메서드
    private void CreateUnit(Tower towerData, GameObject unitPrefab)
    {
        // 기존 오브젝트가 있으면 제거


        // 새로 설치할 오브젝트 생성
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
        installedObject = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

        GameManager.instance.PlaySFX(3,0.5f,1f);

        // 정보 초기화
        InstalledUnit newUnitScript = installedObject.GetComponent<InstalledUnit>();
        if (newUnitScript != null)


        {
            newUnitScript.Initialize(towerData, false); // 실제 설치 모드로 초기화
            



            // 스프라이트와 색상 towerData에서 가져와서 설정
            SpriteRenderer unitSpriteRenderer = installedObject.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null && towerData.unitSprite != null)
            {
                unitSpriteRenderer.sprite = towerData.unitSprite;
                unitSpriteRenderer.color = towerData.unitColor; // towerData의 색상 사용
                Debug.Log($"CreateUnit: 스프라이트 및 색상 설정 완료 - {towerData.unitSprite.name}, 색상: {towerData.unitColor}");
            }
        }
        else
        {
            Debug.LogError($"CreateUnit: InstalledUnit 컴포넌트를 찾을 수 없습니다.");
        }
    }


    // 설치된 유닛만 제거
    public void Remove()
    {
        // 생성 프로세스 중단
        StopAllCoroutines();
        isSpawning = false;

        // 설치된 유닛 오브젝트 파괴
        if (installedObject != null)
        {
            Destroy(installedObject);
            installedObject = null;
        }

        installedUnit = null;
        unitPrefab = null;
        isInstallable = true; // 다시 설치 가능하도록 변경

        Debug.Log("유닛과 무한 생성이 제거되었습니다");
    }

    // 설치구역 자체를 파괴
    public void DestroyZone()
    {
        // 이미 파괴 중인지 확인
        if (this == null) return;
        
        Remove(); 

        // GameManager에 파괴 알림
        if (GameManager.instance != null)
        {
            GameManager.instance.OnInstallZoneDestroyed(this);
        }
        else
        {
            Debug.LogWarning("[InstallZone] GameManager를 찾을 수 없습니다!");
        }
        
        // Tower 오브젝트가 존재하는지 확인 후 파괴
        if (TowerPrefab2 != null)
        {
            try
            {
                Destroy(TowerPrefab2);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InstallZone] Tower 파괴 중 오류: {e.Message}");
            }
        }
        
        // 쿨타임바 파괴
        if (hpBarInstance2 != null)
        {
            try
            {
                Destroy(hpBarInstance2.gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InstallZone] 쿨타임바 파괴 중 오류: {e.Message}");
            }
        }
        
        Debug.Log("설치구역이 파괴되었습니다.");
        click C=GetComponent<click>();
        if (C.levelupUI2_1 != null)
        {
            C.levelupUI2_1.SetActive(false);
        }
        // 마지막에 자기 자신 파괴
        Destroy(gameObject);
    }

    public void DecreaseHp(int damage)
    {
        if (hpBar != null)
        {
            hpBar.decreaseHp(damage);
            if (hpBar.IsDead())
            {
                DestroyZone();
            }
        }
        else
        {
            Debug.LogWarning("[InstallZone] hpBar가 null입니다. 파괴를 진행합니다.");
            DestroyZone();
        }
    }

    // 레이캐스트 방식으로 드롭 처리 (UI에서 3D 오브젝트로)
    public void HandleRaycastDrop(Uiunit uiUnit)
    {
        Debug.Log("InstallZone: HandleRaycastDrop 호출됨 - " + uiUnit.name);
        ProcessDrop(uiUnit);
    }



    // 공통 드롭 처리 로직
    private void ProcessDrop(Uiunit uiUnit)
    {
        if (uiUnit == null)
        {
            Debug.LogError("ProcessDrop: uiUnit이 null입니다.");
            return;
        }
        
        if (uiUnit.towerData == null)
        {
            Debug.LogError("ProcessDrop: uiUnit.towerData가 null입니다.");
            return;
        }
        
        if (uiUnit.unitPrefab == null)
        {
            Debug.LogError("ProcessDrop: uiUnit.unitPrefab이 null입니다.");
            return;
        }

        // 이미 설치가 이루어진 경우 추가 설치를 막음
        if (!isInstallable || installedUnit != null || installedObject != null || isSpawning)
        {
            Debug.Log("설치 불가: 이미 설치가 완료된 존입니다.");
            return;
        }

        Debug.Log($"드롭된 유닛: {uiUnit.towerData.name}");

        // InstallUnit 메서드를 사용하여 설치
        InstallUnit(uiUnit.towerData, uiUnit.unitPrefab);

    }
    public void levelup(Exhence e)
    {
        if (e.type == ExhenceType.unitTower)
        {
            level++;
            attackRange += e.attackRangeBonus;
            attackSpeed += e.attackSpeedBonus;
            hp += e.hpBonus;
            attack += e.attackBonus;
            if (level == 2)
            TowerPrefab2.GetComponent<SpriteRenderer>().sprite = towerData.TowerSprite2;
            if (level == 3)
            TowerPrefab2.GetComponent<SpriteRenderer>().sprite = towerData.TowerSprite3;
        }
        else if (e.type == ExhenceType.goldTower)

        {
            
            level++;
            additionalGold += e.additionalGold;
             GameManager.instance.GoldIncrease(additionalGold);
        }
    }

    public void SetUnitInfo(InstallZone unit)
    {
        if (pandaslot.instance != null)
        {
            pandaslot.instance.SetUnitInfo(unit);
        }
        else
        {
            Debug.LogWarning("[InstallZone] pandaslot.instance가 null입니다. UI 업데이트를 건너뜁니다.");
        }
    }

  /// <summary>
    /// Exhence를 적용하여 유닛 능력치 강화
    /// </summary>
    /// <param name="exhence">적용할 Exhence</param>
    public void ApplyExhence(Exhence exhence)
    {
        if (exhence == null) return;
        
        exhenceList.Add(exhence);
        level++;
        
        Debug.Log($"Exhence 적용: {exhence.exhenceName}, 레벨: {level}");
    }
    
    /// <summary>
    /// Exhence가 적용된 능력치 계산
    /// </summary>
    public int GetTotalHp()
    {
        int totalHp = hp;
        foreach (var e in exhenceList)
        {
            totalHp += e.hpBonus;
        }
        return totalHp;
    }
    
    public int GetTotalAttack()
    {
        int totalAttack = attack;
        foreach (var e in exhenceList)
        {
            totalAttack += e.attackBonus;
        }
        return totalAttack;
    }
    
    public float GetTotalAttackRange()
    {
        float totalRange = attackRange;
        foreach (var e in exhenceList)
        {
            totalRange += e.attackRangeBonus;
        }
        return totalRange;
    }
    
    public float GetTotalAttackSpeed()
    {
        float totalSpeed = attackSpeed;
        foreach (var e in exhenceList)
        {
            totalSpeed += e.attackSpeedBonus;
        }
        return totalSpeed;
    }
}
    
