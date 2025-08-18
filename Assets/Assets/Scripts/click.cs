using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class click : MonoBehaviour
{
    public InstallZone installZone; // InstallZone 컴포넌트 참조

    
    
    public GameObject levelupUI2;
    public GameObject levelupUI2_1;


    void Update()
    {
       
    }
    

    void Start()
    {
        // InstallZone 컴포넌트 가져오기
        installZone = GetComponent<InstallZone>();
        var ps = pandaslot.GetOrFind();
        if (ps != null)
        {
            ps.gameObject.SetActive(false);
        }
    }

    void OnMouseEnter()
    {

        // installed unit이 있는지 체크
        if (installZone != null && installZone.installedUnit != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0, 1, 0);
            levelupUI2_1 = Instantiate(levelupUI2, spawnPosition, Quaternion.identity);
            Debug.Log($"생성된 levelupUI 위치: {levelupUI2_1.transform.position}");
            
            levelupUI2_1.GetComponent<loading>().up(installZone);
            var ps = pandaslot.GetOrFind();
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("pandaslot UI를 찾을 수 없습니다.");
            }
            GameManager.instance.PlaySFX(1,0.5f,1f);

            installZone.SetUnitInfo(installZone);
           
        }
        Debug.Log("OnMouseEnter");
    }

    void OnMouseExit()
    {
        if (pandaslot.GetOrFind() != null)
        {
            pandaslot.GetOrFind().gameObject.SetActive(false);
        }
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
                GameManager.instance.PlaySFX(2,0.5f,1f);
            }
        }
    }

    void OnDestroy()
    {
        // 이 오브젝트가 파괴될 때 생성된 프리팹도 함께 파괴
        if (levelupUI2_1 != null)
        {
            Destroy(levelupUI2_1);
        }
    }
}
