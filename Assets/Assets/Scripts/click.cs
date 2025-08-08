using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class click : MonoBehaviour
{
    public GameObject uiPrefab1;    // 띄울 UI 프리팹 1
    public InstallZone installZone; // InstallZone 컴포넌트 참조

    
    
    public GameObject levelupUI2;
    public GameObject levelupUI2_1;
    

    void Start()
    {
        // InstallZone 컴포넌트 가져오기
        
        installZone = GetComponent<InstallZone>();
        uiPrefab1.SetActive(false);
    }

    void OnMouseEnter()
    {

        // installed unit이 있는지 체크
        if (installZone.installedUnit != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0, 1, 0);
            levelupUI2_1 = Instantiate(levelupUI2, spawnPosition, Quaternion.identity);
            Debug.Log($"생성된 levelupUI 위치: {levelupUI2_1.transform.position}");
            
            levelupUI2_1.GetComponent<loading>().up(installZone);
            uiPrefab1.SetActive(true);

            installZone.SetUnitInfo(installZone);
            Debug.Log("uiPrefab1.SetActive(true)");
        }
        Debug.Log("OnMouseEnter");
    }

    void OnMouseExit()
    {
        uiPrefab1.SetActive(false);
        Destroy(levelupUI2_1);
    }

    void OnMouseDown()
    {
        
        // installed unit이 있고 levelupUI가 있으면 강화 시작
        if (installZone.installedUnit != null && levelupUI2_1 != null)
        {
            loading loadingComponent = levelupUI2_1.GetComponent<loading>();
            if (loadingComponent != null && !loadingComponent.IsUpgrading())
            {
                loadingComponent.StartUpgrade();
                Debug.Log("강화 시작!");
            }
        }
    }
}
