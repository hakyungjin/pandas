using UnityEngine;
using UnityEngine.UI;

public class loading : MonoBehaviour
{
    public Image fillImage;     // 초록색 이미지 (Image Type: Filled)

    public Image background;
    public float duration = 3f; // 총 강화 시간 (초)
    public int upgradeCost = 0;
    private float timer = 0f;
    private bool isUpgrading = false;


    
    // InstallZone 참조
    public InstallZone installZone;
    public Canvas canvas;
    public Color upcolor;



    void Start()
    {
        // Canvas 컴포넌트가 있는지 확인 후 설정
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            // 월드 좌표로 통일
            canvas.transform.position = transform.position + new Vector3(0.5f, -0.5f, 0);
            canvas.sortingOrder = 10;
            Debug.Log($"Canvas 월드 좌표 설정: {canvas.transform.position}");
        }
        else
        {
            Debug.LogWarning("Canvas 컴포넌트를 찾을 수 없습니다. Canvas가 필요합니다.");
        }



        // 초기 fill 값을 0으로 설정
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }


    public void up(InstallZone installZone)
    {
        this.installZone=installZone;
        RecalculateUpgradeCost();
    }

    void Update()
    {
        // 강화 진행 - UI만 변경
        if (isUpgrading)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            fillImage.fillAmount = progress;

            if (progress >= 1f)
            {
                isUpgrading = false;
                Debug.Log("강화 완료!");
                GameManager.instance.PlaySFX(3,0.5f,1f);
                background.color = upcolor;
                ResetFill();
                
                // 유효한 업그레이드인지 확인
                if (installZone != null && installZone.level<3)
                {
                    // 강화 완료 시 골드 차감
                    GameManager.instance.SpendGold(upgradeCost);
                    
                    installZone.levelup(installZone.towerData.exhenceList[installZone.level-1]);
                    RecalculateUpgradeCost();
                }
                else
                {
                    Debug.LogWarning("더 이상 강화할 수 없습니다. 최대 레벨입니다.");
                }
                //1.5초뒤
                Invoke("DestroyObject", 0.5f);
                
            }
        }
    }


    public void StartUpgrade()
    {
        // GameManager 찾기

        // 골드 체크만 (차감은 완료 후에)
        // 업그레이드 가능 여부 확인
        bool canUpgrade = installZone != null && installZone.level < 3;
        if (!canUpgrade)
        {
            Debug.LogWarning("업그레이드 불가: 최대 레벨이거나 데이터가 없습니다.");
            SetRedColor();
            return;
        }

        if (GameManager.instance.GetCurrentGold() >= upgradeCost)
        {
            timer = 0f;
            isUpgrading = true;
            fillImage.fillAmount = 0f;
        }
        else
        {
            Debug.Log("upgradeCost: " + upgradeCost);
            // 골드 부족 시 프리팹 색깔을 빨간색으로 변경
            SetRedColor();
        }
    }

    // 골드 부족 시 빨간색으로 변경하는 메서드
    private void SetRedColor()
    {
        // fillImage를 빨간색으로 변경
        if (fillImage != null)
        {
            fillImage.color = Color.red;
        }
        
        // Canvas의 모든 Image 컴포넌트를 빨간색으로 변경
        Image[] allImages = GetComponentsInChildren<Image>();
        foreach (Image img in allImages)
        {
            img.color = Color.red;
        }
    }
    
    public bool IsUpgrading()
    {
        return isUpgrading;
    }
    
    public void ResetFill()
    {
        if (!isUpgrading)
        {
            fillImage.fillAmount = 0f;
        }
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
    
    private void RecalculateUpgradeCost()
    {
        if (installZone != null && installZone.level<2)
        {
            upgradeCost = installZone.towerData.exhenceList[installZone.level-1].goldcost;
            Debug.Log("upgradeCost: " + upgradeCost);
        }
        else
        {
            upgradeCost = 0;
        }
    }
    
}