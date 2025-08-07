using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class click : MonoBehaviour
{
    public GameObject uiPrefab1;    // 띄울 UI 프리팹 1
    public InstallZone installZone; // InstallZone 컴포넌트 참조

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
            uiPrefab1.SetActive(true);
            installZone.SetUnitInfo(installZone);

            Debug.Log("uiPrefab1.SetActive(true)");
        }
        Debug.Log("OnMouseEnter");
    }

    void OnMouseExit()
    {
        uiPrefab1.SetActive(false);
    }

    
}
