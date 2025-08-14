using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic; // Added for List

public class waveManager : MonoBehaviour
{
    [Header("게이지 설정")]
    public Image hp;
    public float maxFillTime = 240f; // 4분 (240초)
    private float currentTime = 0f;

    [Header("경고 바 설정")]
    public Image warningBar;
    public TextMeshProUGUI warningText; // WARNING 텍스트 컴포넌트
    
    [Header("텍스트 클론 설정")]
    public float cloneMoveSpeed = 60f; // 클론 이동 속도 (픽셀/초)
    public float cloneLifetime = 3f; // 클론 생존 시간 (초)
    public float cloneSpawnInterval = 0.5f; // 클론 생성 간격 (초)

    [Header("이펙트 및 오브젝트")]
    public GameObject targetObject;
    private Transform moveTransform;
    
    public GameObject moveObject;

    public GameObject alert;
    
    // 이동 설정 (로컬 좌표계)
    public Vector3 startPosition = new Vector3(-1.35f, 0f);
    public Vector3 endPosition = new Vector3(1.7f, 0f);

    private bool isGaugeFull => hp.fillAmount >= 1f;
    
    // 경고 트리거 제어 플래그
    private bool firedAt60 = false;
    private bool firedAt240 = false;
    private bool isAlertPlaying = false;
    
    // 텍스트 애니메이션 관련 변수들
    private bool isWarningTextActive = false;
    private float textAnimationTime = 0f;
    private float lastCloneSpawnTime = 0f;
    
    // 텍스트 클론 관리
    private List<TextClone> activeClones = new List<TextClone>();
    
    // 텍스트 클론 클래스
    [System.Serializable]
    public class TextClone
    {
        public GameObject cloneObject;
        public TextMeshProUGUI cloneText;
        public RectTransform cloneRectTransform;
        public float spawnTime;
        public Vector2 startPosition;
        
        public TextClone(GameObject obj, TextMeshProUGUI text, RectTransform rect, Vector2 startPos)
        {
            cloneObject = obj;
            cloneText = text;
            cloneRectTransform = rect;
            spawnTime = Time.time;
            startPosition = startPos;
        }
    }

    [Header("경고 UI 이동 설정")]
    public float alertMoveDistance = 100f;      // 위로 이동 거리 (UI 좌표)
    public float alertMoveDuration = 0.35f;     // 위로/아래로 이동 시간
    public AnimationCurve alertEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 이징 커브
    public float alertHoldTime = 0.2f;          // 꼭대기/바닥에서 머무는 시간

    void Start()
    {
        alert.SetActive(false);
        hp.fillAmount = 0f;
        currentTime = 0f;
        
        // 워닝바 초기화
        if (warningBar != null)
        {
            warningBar.gameObject.SetActive(false);
        }
        
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

        // WARNING 텍스트 업데이트
        if (isWarningTextActive)
        {
            UpdateWarningText();
            UpdateTextClones();
        }

        if (!firedAt60 && currentTime >= 50f)
        {
            warningBar.gameObject.SetActive(true);
            isWarningTextActive = true;
            textAnimationTime = 0f;
        }

        if (!firedAt60 && currentTime >= 60f)
        {
            firedAt60 = true;
            warningBar.gameObject.SetActive(false);
            isWarningTextActive = false;
            ClearAllTextClones();
            TriggerWaveEffect();
        }
        
        // 두 번째 웨이브 (240초) 워닝바 처리
        if (!firedAt240 && currentTime >= 230f)
        {
            warningBar.gameObject.SetActive(true);
            isWarningTextActive = true;
            textAnimationTime = 0f;
        }
        
        if(!firedAt240 && currentTime >= 240f)
        {
            firedAt240 = true;
            warningBar.gameObject.SetActive(false);
            isWarningTextActive = false;
            ClearAllTextClones();
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

    // WARNING 텍스트 업데이트
    void UpdateWarningText()
    {
        if (warningText != null && isWarningTextActive)
        {
            textAnimationTime += Time.deltaTime;
            
            // 일정 간격으로 클론 생성
            if (Time.time - lastCloneSpawnTime >= cloneSpawnInterval)
            {
                CreateTextClone();
                lastCloneSpawnTime = Time.time;
            }
        }
    }

    // 텍스트 클론 생성
    void CreateTextClone()
    {
        if (warningText == null) return;
        
        // 원본 텍스트의 위치에서 클론 생성
        Vector2 spawnPosition = warningText.GetComponent<RectTransform>().anchoredPosition;
        
        // 클론 오브젝트 생성
        GameObject cloneObj = Instantiate(warningText.gameObject, warningText.transform.parent);
        TextMeshProUGUI cloneText = cloneObj.GetComponent<TextMeshProUGUI>();
        RectTransform cloneRect = cloneObj.GetComponent<RectTransform>();
        
        if (cloneText != null && cloneRect != null)
        {
            // 클론 설정
            cloneText.text = "WARNING";
            cloneRect.anchoredPosition = spawnPosition;
            
            // 클론 정보 저장
            TextClone textClone = new TextClone(cloneObj, cloneText, cloneRect, spawnPosition);
            activeClones.Add(textClone);
            

        }
    }

    // 텍스트 클론 업데이트
    void UpdateTextClones()
    {
        // 오래된 클론들 제거
        for (int i = activeClones.Count - 1; i >= 0; i--)
        {
            TextClone clone = activeClones[i];
            
            // 생존 시간 체크
            if (Time.time - clone.spawnTime >= cloneLifetime)
            {
                Destroy(clone.cloneObject);
                activeClones.RemoveAt(i);
                continue;
            }
            
            // 클론 이동
            MoveTextClone(clone);
        }
    }

    // 텍스트 클론 이동
    void MoveTextClone(TextClone clone)
    {
        if (clone.cloneRectTransform != null)
        {
            // X좌표만 이동
            float elapsedTime = Time.time - clone.spawnTime;
            float xOffset = elapsedTime * cloneMoveSpeed;
            
            Vector2 newPosition = clone.startPosition;
            newPosition.x += xOffset;
            
            clone.cloneRectTransform.anchoredPosition = newPosition;
        }
    }

    // 모든 텍스트 클론 정리
    void ClearAllTextClones()
    {
        foreach (TextClone clone in activeClones)
        {
            Destroy(clone.cloneObject);
        }
        activeClones.Clear();
    }

    public void TriggerWaveEffect()
    {
        if (isAlertPlaying) return;
        StartCoroutine(WaveEffectCoroutine());
    }

    IEnumerator WaveEffectCoroutine()
    {
        isAlertPlaying = true;

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }

        if (alert != null)
        {
            alert.SetActive(true);

            RectTransform rt = alert.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 baseAnchored = rt.anchoredPosition;
                // 위로 부드럽게 이동
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.0001f, alertMoveDuration);
                    float eased = alertEaseCurve != null ? alertEaseCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                    rt.anchoredPosition = baseAnchored + Vector2.up * (alertMoveDistance * eased);
                    yield return null;
                }
                // 꼭대기에서 잠깐 유지
                if (alertHoldTime > 0f) yield return new WaitForSeconds(alertHoldTime);
                // 아래로 부드럽게 복귀
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.0001f, alertMoveDuration);
                    float eased = alertEaseCurve != null ? alertEaseCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                    rt.anchoredPosition = baseAnchored + Vector2.up * (alertMoveDistance * (1f - eased));
                    yield return null;
                }
                // 바닥에서 잠깐 유지
                if (alertHoldTime > 0f) yield return new WaitForSeconds(alertHoldTime);
                rt.anchoredPosition = baseAnchored;
            }
            else
            {
                Vector3 baseWorld = alert.transform.position;
                float t = 0f;
                // 위로 부드럽게 이동 (월드 좌표)
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.0001f, alertMoveDuration);
                    float eased = alertEaseCurve != null ? alertEaseCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                    alert.transform.position = baseWorld + Vector3.up * (alertMoveDistance * eased);
                    yield return null;
                }
                if (alertHoldTime > 0f) yield return new WaitForSeconds(alertHoldTime);
                // 아래로 부드럽게 복귀
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / Mathf.Max(0.0001f, alertMoveDuration);
                    float eased = alertEaseCurve != null ? alertEaseCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                    alert.transform.position = baseWorld + Vector3.up * (alertMoveDistance * (1f - eased));
                    yield return null;
                }
                if (alertHoldTime > 0f) yield return new WaitForSeconds(alertHoldTime);
                alert.transform.position = baseWorld;
            }

            alert.SetActive(false);
        }

        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }

        isAlertPlaying = false;
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
