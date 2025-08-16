using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UnitSelector : MonoBehaviour
{
    public RectTransform selectionBox;
    

    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private bool isSelecting = false;

    private bool unitdraged=false;

    private List<SelectedUnit> selectedUnits = new List<SelectedUnit>();

    public static UnitSelector instance;

    void Awake()
    {
      //싱글톤 패턴으로 구현
      if(instance==null)
      {
        instance=this;
      }
      else
      {
        Destroy(gameObject);
      }
        
    }

    public void setUnitDraged(bool isDraged)
    {
        unitdraged=isDraged;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;

        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Count > 0;
    }

    public void selectUnit(SelectedUnit unit)
    {
        selectedUnits.Add(unit);
    }

    // 현재 선택된 유닛들을 반환하는 메서드
    public List<SelectedUnit> GetSelectedUnits()
    {
        List<SelectedUnit> currentlySelected = new List<SelectedUnit>();
        
        // 파괴된 유닛들을 제거하기 위해 역순으로 순회
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            SelectedUnit unit = selectedUnits[i];
            
            // null 체크 - 파괴된 유닛은 리스트에서 제거
            if (unit == null)
            {
                selectedUnits.RemoveAt(i);
                continue;
            }
            
            if (unit.isSelected)
            {
                currentlySelected.Add(unit);
            }
        }
        return currentlySelected;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !unitdraged && !IsPointerOverUI())
        {
            isSelecting = true;
            startMousePos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0) )
        {
            
            endMousePos = Input.mousePosition;
            UpdateSelectionBox(startMousePos, endMousePos);

        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            selectionBox.gameObject.SetActive(false);
            SelectUnits();
            startMousePos = Vector2.zero;
            endMousePos = Vector2.zero;
            UpdateSelectionBox(startMousePos, endMousePos);
        }
        
                
               
                }
        
    

    void UpdateSelectionBox(Vector2 start, Vector2 endpos)
    {
        // 스크린 좌표를 UI 좌표로 변환
        Vector2 uiStart, uiEnd;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, start, null, out uiStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform, endpos, null, out uiEnd);

        // 크기 계산
        Vector2 size = uiEnd - uiStart;

        // 드래그 방향에 따라 pivot 설정 (시작점 기준)
        Vector2 pivot = new Vector2(
            size.x >= 0 ? 0 : 1,  // 오른쪽 드래그면 왼쪽 기준, 왼쪽 드래그면 오른쪽 기준
            size.y >= 0 ? 0 : 1   // 위로 드래그면 아래 기준, 아래로 드래그면 위 기준
        );

        // 시작점을 기준으로 설정
        selectionBox.pivot = pivot;
        selectionBox.anchoredPosition = uiStart;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    void SelectUnits()
    {
        // 선택 영역의 최소/최대 스크린 좌표 계산
        Vector2 min = Vector2.Min(startMousePos, endMousePos);
        Vector2 max = Vector2.Max(startMousePos, endMousePos);

        // 파괴된 유닛들을 제거하기 위해 역순으로 순회
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            SelectedUnit unit = selectedUnits[i];
            
            // null 체크 - 파괴된 유닛은 리스트에서 제거
            if (unit == null)
            {
                selectedUnits.RemoveAt(i);
                continue;
            }

            // 유닛의 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            
            // 카메라 뒤에 있는 오브젝트는 제외
            if (screenPos.z < -10) 
            {
                unit.Deselect();
                continue;
            }

            // 선택 영역 안에 있는지 확인
            if (screenPos.x >= min.x && screenPos.x <= max.x &&
                screenPos.y >= min.y && screenPos.y <= max.y)
            {
                unit.Select();
            }
            else
            {
                unit.Deselect();
            }
        }
    }
}
