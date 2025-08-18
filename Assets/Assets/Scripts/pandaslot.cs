using UnityEngine;
using UnityEngine.UI;
using TMPro;  
using UnityEngine.UI;


public class pandaslot : MonoBehaviour
{
     public Image unitImage;
     public TextMeshProUGUI hpText;
     public TextMeshProUGUI atkText;
     public TextMeshProUGUI rngText;
     public TextMeshProUGUI speedText;
     public TextMeshProUGUI levelText;
     public Image[] levelup;



    public static pandaslot instance;

    void Awake()
    {
        instance = this;
    }

    public static pandaslot GetOrFind()
    {
        if (instance != null) return instance;
        var all = Resources.FindObjectsOfTypeAll<pandaslot>();
        foreach (var s in all)
        {
            if (s != null && s.gameObject.scene.IsValid())
            {
                instance = s;
                break;
            }
        }
        return instance;
    }



    public void SetUnitInfo(InstallZone unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("[pandaslot] SetUnitInfo: unit이 null입니다.");
            return;
        }

        if (unitImage == null || hpText == null || atkText == null || rngText == null || speedText == null || levelText == null)
        {
            Debug.LogError("[pandaslot] UI 레퍼런스가 비어 있습니다. Inspector에서 unitImage/hpText/atkText/rngText/speedText/levelText를 연결하세요.");
            return;
        }

        // 아이콘이 없으면 유닛 스프라이트로 대체
        unitImage.sprite = unit.iconSprite != null ? unit.iconSprite : unit.unitSprite;
        hpText.text = unit.GetTotalHp().ToString();
        atkText.text = unit.GetTotalAttack().ToString();
        rngText.text = unit.GetTotalAttackRange().ToString("F1");
        speedText.text = unit.GetTotalAttackSpeed().ToString("F1");
        levelText.text = unit.level.ToString();

        if (levelup == null || levelup.Length < 3)
        {
            Debug.LogWarning("[pandaslot] levelup 아이콘 배열이 비어있거나 3개 미만입니다.");
            return;
        }

        if (unit.level <= 1)
        {
            levelup[0].gameObject.SetActive(true);
            levelup[1].gameObject.SetActive(false);
            levelup[2].gameObject.SetActive(false);
        }
        else if (unit.level == 2)
        {
            levelup[0].gameObject.SetActive(true);
            levelup[1].gameObject.SetActive(true);
            levelup[2].gameObject.SetActive(false);
        }
        else
        {
            levelup[0].gameObject.SetActive(true);
            levelup[1].gameObject.SetActive(true);
            levelup[2].gameObject.SetActive(true);
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }


     }
    

