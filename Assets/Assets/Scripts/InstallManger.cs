using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class InstallManager : MonoBehaviour
{
    

    [Header("타일 설치 설정")]
    public GameObject tilePrefab;
    public GameObject deleteButtonPrefab; // 삭제 버튼 프리팹 (인스펙터에서 할당)
    public Canvas targetCanvas; // 대상 Canvas (인스펙터에서 할당)

    private Camera mainCamera;
    private GameObject currentDeleteButton; // 현재 생성된 삭제 버튼

    private float lastClickTime = 0f;
    private float doubleClickDelay = 0.4f; // 더블클릭 간격(초)

    private bool isshow=false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    void Update()
    {
        


        // 마우스 우클릭으로 타일 삭제
        if (Input.GetMouseButtonDown(1))
        {
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            // 타일 위에 있는지 확인
            if (IsPositionOccupied(mousePos))
            {
                // 타일 위에서 우클릭한 경우에만 메뉴 표시
                ShowMenu();
            }
        }
        if(Input.GetMouseButtonDown(0))
        { var pos=new PointerEventData(EventSystem.current);
          pos.position=Input.mousePosition;
          List<RaycastResult> results=new List<RaycastResult>();
          EventSystem.current.RaycastAll(pos,results);
          foreach(RaycastResult result in results)
          {
            if(result.gameObject==currentDeleteButton)
            {return;}
          }
          if(currentDeleteButton!=null)
          { 
            Destroy(currentDeleteButton);
            currentDeleteButton=null;
          }
        }
    }

    // 우클릭 시 컨텍스트 메뉴 표시
    void ShowMenu()
    {
        if (deleteButtonPrefab == null)
        {
            Debug.LogWarning("deleteButtonPrefab이 할당되지 않았습니다!");
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("targetCanvas가 할당되지 않았습니다!");
            return;
        }
        isshow=true;
        // 기존 버튼이 있으면 제거
        if (currentDeleteButton != null)
        {
            Destroy(currentDeleteButton);
        }

        // 마우스 위치를 화면 좌표로 변환
        Vector3 mouseScreenPos = Input.mousePosition;

        // Canvas 하위에 버튼 생성
        currentDeleteButton = Instantiate(deleteButtonPrefab, mouseScreenPos, Quaternion.identity, targetCanvas.transform);
        
        // 버튼 이벤트 연결
        Button buttonComponent = currentDeleteButton.GetComponentInChildren<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(OnButtonClick);
        }
    }

    // UI 숨기기 (다른 곳 클릭 시)
    public void HideMenu()
    {
        isshow=false;
        if (currentDeleteButton != null)
        {
            Destroy(currentDeleteButton);
            currentDeleteButton = null;
        }
    }

    public void InstallUnit(Transform transform)
    {
        // 프리팹이 할당되지 않았으면 리턴
        if (tilePrefab == null)
        {
            Debug.LogWarning("tilePrefab이 할당되지 않았습니다!");
            return;
        }

        

        // 겹침 검사
        if (IsPositionOccupied(transform.position))
        {
            Debug.Log("이미 타일이 설치된 위치입니다!");
            return;
        }

        // InstallManager 하위에 프리팹 생성
        GameObject newUnit = Instantiate(tilePrefab, transform.position, Quaternion.identity, this.transform);

        // 장판 개수 업데이트
        UpdateTileCount();
    }

    // 위치에 이미 타일이 있는지 검사
    bool IsPositionOccupied(Vector3 position)
    {
        // InstallManager 하위의 모든 자식 오브젝트 확인
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            // 마우스 위치와 타일 위치의 거리 계산
            float distance = Vector2.Distance(position, child.position);
            
            // 일정 거리 내에 이미 타일이 있으면 겹침으로 판단
            if (distance < 2f) // 겹침 판정 거리 (타일 크기에 따라 조정)
            {
                return true;
            }
        }
        return false;
    }

    // 버튼 클릭 시 실행되는 함수
    public void OnButtonClick()
    {
        Debug.Log("버튼이 클릭되었습니다!");
        DeleteTile();
    }

    public void DeleteTile()
    {
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // InstallManager 하위의 모든 자식 오브젝트 확인
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            // 마우스 위치와 타일 위치의 거리 계산
            float distance = Vector2.Distance(mousePos, child.position);

            // 일정 거리 내에 있으면 삭제 (타일 크기에 따라 조정)
            if (distance < 1f) // 1f는 타일 크기에 맞게 조정
            {
                Destroy(child.gameObject);

                // 장판 개수 업데이트
                UpdateTileCount();
                
                // 메뉴 숨기기
                HideMenu();
                
                Debug.Log("타일이 삭제되었습니다.");
                return; // 첫 번째 발견된 타일만 삭제
            }
        }
    }
    

    // InstallManager 하위의 장판 개수를 계산하여 GameManager에 전달
    void UpdateTileCount()
    {
        // InstallManager 하위의 모든 자식 오브젝트 중 tilePrefab과 같은 타입의 개수 계산
        int count = 0;
        
        // 현재 하위에 있는 모든 자식 오브젝트 확인
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // tilePrefab과 같은 이름을 가진 오브젝트들만 카운트
            count++;
        }
        
        // GameManager에 개수 전달
        if (GameManager.Instance != null)
        {
            GameManager.Instance.tileCount += count;
            Debug.Log($"장판 개수 업데이트: {count}개");
        }
    }
    
}
