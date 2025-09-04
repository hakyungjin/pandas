using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class tutorial : MonoBehaviour
{
    void Update()
    {
        // 엔터키 입력 감지
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleUI();
        }
    }



    void ToggleUI()
    {
        
    }



}
