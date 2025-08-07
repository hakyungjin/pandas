using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class waveManager : MonoBehaviour
{
    [Header("게이지 설정")]
    public Image hp;
    public float maxFillTime = 240f; // 4분 (240초)
    private float currentTime = 0f;

    [Header("이펙트 및 오브젝트")]
    public GameObject targetObject;
    private Transform moveTransform;
    
    public GameObject moveObject;

    public GameObject alert;
    
    // 이동 설정 (로컬 좌표계)
    public Vector3 startPosition = new Vector3(-1.35f, 0f);
    public Vector3 endPosition = new Vector3(1.7f, 0f);

    private bool isGaugeFull => hp.fillAmount >= 1f;

    void Start()
    {
        alert.SetActive(false);
        hp.fillAmount = 0f;
        currentTime = 0f;
        
        // Transform 컴포넌트 가져오기
        if (moveObject != null)
        {
            moveTransform = moveObject.GetComponent<Transform>();
            if (moveTransform != null)
            {
                // 시작 위치 설정 (로컬 좌표)
                moveTransform.localPosition = startPosition;
            }
        }
    }

    void Update()
    {
        if (!isGaugeFull)
        {
            currentTime += Time.deltaTime;
            hp.fillAmount = Mathf.Clamp01(currentTime / maxFillTime);
            
            // 타겟 오브젝트 이동
            MoveTargetObject();
        }
        
        if(currentTime >= 60f && currentTime < 61f)
        {
            TriggerWaveEffect();
        }
        if(currentTime >= 240f && currentTime < 241f)
        {
            TriggerWaveEffect();
        }
    }

    void MoveTargetObject()
    {
        if (moveTransform != null)
        {
            // 게이지 진행률에 따라 위치 계산 (로컬 좌표)
            float progress = hp.fillAmount;
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, progress);
            moveTransform.localPosition = newPosition;
        }
    }

    public void TriggerWaveEffect()
    {
        if (!isGaugeFull) return;

        if (targetObject != null)
        {
            StartCoroutine(WaveEffectCoroutine());
        }
    }

    IEnumerator WaveEffectCoroutine()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            alert.SetActive(true);
            //위로올라감    
            alert.transform.position += new Vector3(0f, 100f, 0f);
            yield return new WaitForSeconds(0.5f);
            //아래로내려감
            alert.transform.position -= new Vector3(0f, 100f, 0f);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(1f);
            targetObject.SetActive(true);
            alert.SetActive(false);
        }
    }
    
    // 위치 설정을 위한 헬퍼 메서드들
    [ContextMenu("현재 위치를 시작점으로 설정")]
    void SetCurrentAsStart()
    {
        if (moveTransform != null)
        {
            startPosition = moveTransform.localPosition;
            Debug.Log($"시작 위치를 현재 위치로 설정: {startPosition}");
        }
    }
    
    [ContextMenu("현재 위치를 끝점으로 설정")]
    void SetCurrentAsEnd()
    {
        if (moveTransform != null)
        {
            endPosition = moveTransform.localPosition;
            Debug.Log($"끝 위치를 현재 위치로 설정: {endPosition}");
        }
    }
    
    [ContextMenu("X축 이동으로 설정")]
    void SetXAxisMovement()
    {
        startPosition = new Vector3(-3f, 0f, 0f);
        endPosition = new Vector3(3f, 0f, 0f);
        if (moveTransform != null)
            moveTransform.localPosition = startPosition;
    }
    
    [ContextMenu("Y축 이동으로 설정")]
    void SetYAxisMovement()
    {
        startPosition = new Vector3(0f, -2f, 0f);
        endPosition = new Vector3(0f, 2f, 0f);
        if (moveTransform != null)
            moveTransform.localPosition = startPosition;
    }
}
