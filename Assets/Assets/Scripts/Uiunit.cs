using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Uiunit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    private Canvas canvas;
    
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;  
    public TextMeshProUGUI GoldcostText;
    public Image unitImage;
    public Tower towerData; // 드래그할 유닛 데이터

    
    private bool droppedSuccessfully = false;
    

    public GameObject dragIconPrefab; // 드래그 중 따라다닐 이미지 프리팹
    private GameObject dragIconInstance;
    public GameObject unitPrefab;

    public GameManager gameManager;


    void Awake()
    {
        Debug.Log("[Uiunit] Awake");
       
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.Log("Canvas 컴포넌트를 찾을 수 없습니다!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragIconInstance = Instantiate(dragIconPrefab, canvas.transform);
        dragIconInstance.GetComponent<Image>().sprite = unitImage.sprite;
        dragIconInstance.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f); // 반투명
        GameManager.instance.PlaySFX(1,0.5f,1f);
        
    }

    public void OnDrag(PointerEventData eventData)
    {
       if (dragIconInstance != null)
        {
            RectTransform rt = dragIconInstance.GetComponent<RectTransform>();
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out pos
            );
            rt.localPosition = pos;
        }
    }

   public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance);
            GameManager.instance.PlaySFX(1,0.5f,1f);
        }

        // 드롭된 위치 → 월드 좌표 변환
        Vector3 dropWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dropWorldPos.z = 0;

        RaycastHit2D hit = Physics2D.Raycast(dropWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // 여기에 드롭 성공 시 처리
            Debug.Log("드롭된 대상: " + hit.collider.name);

            // null 체크 추가
            if (towerData == null)
            {
                Debug.LogError("OnEndDrag: towerData가 null입니다.");
                return;
            }
            
            if (unitPrefab == null)
            {
                Debug.LogError("OnEndDrag: unitPrefab이 null입니다.");
                return;
            }

            // 예: 유닛 생성
            var droppable = hit.collider.GetComponent<IDroppable>();
            if (droppable != null)
            {
                droppable.OnDropFromUI(this);
                GameManager.instance.SpendGold(towerData.goldCost);
            }
            else
            {
                Debug.LogWarning("드롭된 오브젝트에 IDroppable 인터페이스가 없습니다.");
            }
        }
        else
        {
            Debug.Log("드롭 실패 (빈 공간)");
        }
    }

    
    


    public void SetTowerInfo(Tower tower)
    {
        if (tower == null)
        {
            Debug.LogError("SetTowerInfo: tower가 null입니다.");
            return;
        }
        
        towerData = tower;
        
        // unitPrefab도 설정해야 합니다
        if (unitPrefab == null)
        {
            Debug.LogWarning("SetTowerInfo: unitPrefab이 설정되지 않았습니다.");
        }
       
        unitImage.sprite = tower.unitSprite;
        GoldcostText.text = tower.goldCost.ToString();
        
        Debug.Log($"SetTowerInfo: {tower.name} 설정 완료, unitPrefab: {(unitPrefab != null ? unitPrefab.name : "null")}");
    }

    
}
