using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class unitManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public GameObject towerUI;
    public List<GameObject> slots; // 미리 만들어둔 슬롯 오브젝트들
    public List<Tower> towerObjects = new List<Tower>();

    void Awake()
    {
        Debug.Log("[unitManager] Awake - 초기화 시작");
    }

    void Start()
    {
        Debug.Log("[unitManager] Start - 슬롯 정보 설정");
        UpdateTowerSlots();
    }

    void Update()
    {
        
    }

    public void UpdateTowerSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < towerObjects.Count)
            {
                slots[i].SetActive(true);
                slots[i].GetComponent<Uiunit>().SetTowerInfo(towerObjects[i]);
                // 추가 정보(아이콘 등)도 여기에 할당
            }
            else
            {
                slots[i].SetActive(false);
            }
        }
    }


     void OnEnable()
    {
        UpdateTowerSlots();
    }

   

    public void OnPointerEnter(PointerEventData eventData)
    {
        UnitSelector.instance.setUnitDraged(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UnitSelector.instance.setUnitDraged(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnitSelector.instance.setUnitDraged(false);
    }


}


