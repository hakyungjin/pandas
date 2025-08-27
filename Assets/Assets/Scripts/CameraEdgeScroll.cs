using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraEdgeScroll : MonoBehaviour
{
   
 

    public float scrollSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 20f;         // 감지할 화면 가장자리 범위 (픽셀)

    public Transform hero;
    public bool isFollowingHero = true;
    public bool isPause = false;

  

    void Update()
    {
        if (!isFollowingHero)
        {
            scrolldown();
        }

        if (GameManager.instance.gameStopUI.activeSelf)
        {
            isPause = !isPause;
        }

        if (isPause)
        {
            HeroPanda.instance.isFollowingHerochange();
            isFollowingHero = !isFollowingHero;
            isPause = !isPause;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Y) && !isPause)
        {
            HeroPanda.instance.isFollowingHerochange();
            isFollowingHero = !isFollowingHero;
        }
        
        if(isFollowingHero)
        {
            if(hero!=null)
            {
                transform.position = hero.position + new Vector3(0, 0, -10);
            }
            else {isFollowingHero=false;}
        }
        
        
    }
    
   
   void scrolldown() {

    Vector3 move = Vector3.zero;

        // 화면 왼쪽 감지
        if (Input.GetKey(KeyCode.A))
            move.x -= 1f;

        // 화면 오른쪽 감지
        if (Input.GetKey(KeyCode.D))
            move.x += 1f;

        // 화면 아래쪽 감지
        if (Input.GetKey(KeyCode.S))
            move.y -= 1f;

        // 화면 위쪽 감지
        if (Input.GetKey(KeyCode.W))
            move.y += 1f;

        // 이동 적용
        transform.position += move * scrollSpeed * Time.unscaledDeltaTime;
   }

   
}
   



