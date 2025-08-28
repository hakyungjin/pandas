using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraEdgeScroll : MonoBehaviour
{
    public float scrollSpeed = 10f;      // 카메라 이동 속도
    public float edgeSize = 20f;         // 감지할 화면 가장자리 범위 (픽셀)

    public Transform hero;
    // 내부변수
    private Animator heroAnimator;
    private bool isDie = false;
    private bool isPause = false;

    void Start()
    {
        heroAnimator = GameManager.instance.heropandaObject.GetComponent<Animator>(); // 영웅판다 애니메이션 참조
        isDie = GameManager.instance.heropandaObject.GetComponent<HeroPanda>().isDie; // 영웅판다 사망 상태 참조
    }

    void Update()
    {
        // 상태 업데이트
        if (GameManager.instance.heropandaObject != null)
        {
            HeroPanda heroPanda = GameManager.instance.heropandaObject.GetComponent<HeroPanda>();
            if (heroPanda != null)
            {
                isDie = heroPanda.isDie;
                isPause = heroPanda.isPause;
            }
        }

        // 영웅이 살아있고 일시정지가 아닐 때만 영웅을 따라감
        if (!isDie && !isPause && hero != null)
        {
            transform.position = hero.position + new Vector3(0, 0, -10);
        }
    }

    public void startscroll()
    {
        heroAnimator.SetFloat("speed", 0);
        heroAnimator.SetFloat("MoveX", 0);
        heroAnimator.SetFloat("MoveY", 0);
        heroAnimator.SetBool("isAttacking", false);
        heroAnimator.SetBool("isSkill1", false);
        heroAnimator.SetBool("isSkill2", false);
        heroAnimator.SetBool("isSkill3", false);
    }

    public void scrolldown()
    {
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

    public void endscroll()
    {
        transform.position = hero.position;
    }
}
   



