using UnityEngine;
using UnityEngine.UI;
using DamageNumbersPro;

public class hpbar : MonoBehaviour
{
    [Header("HP Bar Settings")]
    public Image hpImage;
    public float maxHp = 100f;
    public float currentHp = 100f;

    [Header("Visual Settings")]
    public bool autoFindImage = true;
    public Color fullHpColor;
    public Color lowHpColor;
    public DamageNumber damageNumberPrefab;

    void Start()
    {
        InitializeHpBar();
    }

    void InitializeHpBar()
    {
        // Image 컴포넌트 찾기
        if (hpImage == null && autoFindImage)
        {
            hpImage = GetComponent<Image>();
            if (hpImage == null)
            {
                hpImage = GetComponentInChildren<Image>();
            }
        }

        // Image 컴포넌트가 없으면 경고
        if (hpImage == null)
        {
            Debug.LogWarning("hpbar: Image component not found! Please assign an Image component.");
            return;
        }

        // Image 설정
        hpImage.type = Image.Type.Filled;
        hpImage.fillMethod = Image.FillMethod.Horizontal;
        hpImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        UpdateHpBar();
    }

    public void SetHp(float hp)
    {
        currentHp = Mathf.Clamp(hp, 0f, maxHp);
        UpdateHpBar();
       
    }

    public void SetMaxHp(float maxHp)
    {
        this.maxHp = maxHp;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        UpdateHpBar();
        
    }

    public void decreaseHp(float hp)
    {
        currentHp = Mathf.Clamp(currentHp - hp, 0f, maxHp);
        UpdateHpBar();
        SpawnDamageNumber(hp);
    }

    public void increaseHp(float hp)
    {
        currentHp = Mathf.Clamp(currentHp + hp, 0f, maxHp);
        UpdateHpBar();
    }

    public float GetHpPercentage()
    {
        return maxHp > 0 ? currentHp / maxHp : 0f;
    }

    public bool IsDead()
    {
        return currentHp <= 0f;
    }

    private void UpdateHpBar()
    {
        if (hpImage == null) return;

        float fillAmount = GetHpPercentage();
        hpImage.fillAmount = fillAmount;

        // HP에 따른 색상 변경
        hpImage.color = Color.Lerp(lowHpColor, fullHpColor, fillAmount);
    }

    // 디버그용 - 에디터에서만 작동
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (hpImage != null)
        {
            hpImage.type = Image.Type.Filled;
            hpImage.fillMethod = Image.FillMethod.Horizontal;
            hpImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }
    public void SpawnDamageNumber(float damage)
    {    
        // NULL 체크 추가
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("hpbar: damageNumberPrefab is not assigned!");
            return;
        }
        
        if (Camera.main == null)
        {
            Debug.LogWarning("hpbar: Camera.main is null! Make sure there's a camera with MainCamera tag.");
            return;
        }
        
        // Canvas UI 요소의 올바른 월드 위치 계산
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            // Canvas가 Screen Space - Overlay인 경우
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // UI 요소의 스크린 위치를 월드 위치로 변환
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rectTransform.position);
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                damageNumberPrefab.Spawn(worldPos, damage);
            }
            else if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Screen Space - Camera 모드
                Vector3 worldPos = rectTransform.position;
                worldPos.z = 10f;
                damageNumberPrefab.Spawn(worldPos, damage);
            }
            else
            {
                // World Space 모드 또는 기본값
                Vector3 worldPos = rectTransform.position;
                worldPos.z = 10f;
                damageNumberPrefab.Spawn(worldPos, damage);
            }
        }
        else
        {
            // RectTransform이 아닌 경우 기존 방식 사용 (fallback)
            var sp = new Vector3(transform.position.x, transform.position.y, 10f);
            var wp = Camera.main.ScreenToWorldPoint(sp);
            damageNumberPrefab.Spawn(wp, damage);
        }
    }
}