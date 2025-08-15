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



    public void SetUnitInfo(InstallZone unit)
    {
        unitImage.sprite = unit.unitSprite; 
        hpText.text = unit.GetTotalHp().ToString();
        atkText.text = unit.GetTotalAttack().ToString();
        rngText.text = unit.GetTotalAttackRange().ToString("F1");
        speedText.text = unit.GetTotalAttackSpeed().ToString("F1");
        levelText.text = unit.level.ToString();

        if (unit.level == 1)
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
        else if (unit.level == 3)
        {
            levelup[0].gameObject.SetActive(true);
            levelup[1].gameObject.SetActive(true);
            levelup[2].gameObject.SetActive(true);
        }
    }


     }
    

