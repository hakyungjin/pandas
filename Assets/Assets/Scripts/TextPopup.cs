using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextPopup : MonoBehaviour
{
    [Header("프리팹 설정")]
    public GameObject textPrefab; // 프리팹
    
    [Header("애니메이션 설정")]
    public float displayTime = 0.3f; // 표시 시간
    public float fadeTime = 0.3f; // 페이드 아웃 시간
    public float moveSpeed = 50f; // 위로 올라가는 속도
    
    
    /// <summary>
    /// 텍스트 애니메이션을 실행합니다.
    /// </summary>
    private IEnumerator AnimateText(GameObject textObj)
    {
        // 표시 시간 대기
        yield return new WaitForSeconds(displayTime);
        
        // 페이드 아웃 + 위로 이동
        float elapsedTime = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * moveSpeed;
        
        CanvasGroup canvasGroup = textObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textObj.AddComponent<CanvasGroup>();
        }
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeTime;
            
            // 알파값 감소
            canvasGroup.alpha = 1f - progress;
            
            // 위치 이동
            textObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
            
            yield return null;
        }
        
        // 오브젝트 제거
        Destroy(textObj);
    }
    
    
    /// <summary>
    /// 특정 위치에 텍스트를 표시합니다.
    /// </summary>
    public void ShowTextAtPosition(string message, Vector3 position)
    {
        if (textPrefab != null)
        {
            // 위치 조정 (오프셋 추가 가능)
            Vector3 adjustedPos = position + new Vector3(-60f, -10f, 0);
        
            // 텍스트 오브젝트 생성
            GameObject textObj = Instantiate(textPrefab, adjustedPos, Quaternion.identity, transform);
            
            TextMeshProUGUI textComponent = textObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = message;
            }
            
            // 애니메이션 시작
            StartCoroutine(AnimateText(textObj));
        }
    }
    
    /// <summary>
    /// 특정 위치에 텍스트를 표시하는 정적 메서드
    /// </summary>
    public static void ShowText(string message, Vector3 position)
    {
        TextPopup popup = FindObjectOfType<TextPopup>();
        if (popup != null)
        {
            popup.ShowTextAtPosition(message, position);
        }
    }
}
