using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraEdgeScroll : MonoBehaviour
{
   
 

    public float scrollSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 20f;         // 감지할 화면 가장자리 범위 (픽셀)

  



    

    void Update()
    {
        scrolldown();
        
    }
   
   void scrolldown() {

    Vector3 move = Vector3.zero;

        // 화면 왼쪽 감지
        if (Input.mousePosition.x < edgeSize)
            move.x -= 1f;

        // 화면 오른쪽 감지
        if (Input.mousePosition.x > Screen.width - edgeSize)
            move.x += 1f;

        // 화면 아래쪽 감지
        if (Input.mousePosition.y < edgeSize)
            move.y -= 1f;

        // 화면 위쪽 감지
        if (Input.mousePosition.y > Screen.height - edgeSize)
            move.y += 1f;

        // 이동 적용
        transform.position += move * scrollSpeed * Time.deltaTime;
   }

   
}
   



