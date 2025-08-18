using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Herostate : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Image hpbar;
    public Image expbar;
    public Image skill1;
    public Image skill2;
    public Image skill3;
    
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setstate(float hp,float exp,int level) {

         hpbar.fillAmount=hp;
         expbar.fillAmount=exp;
         levelText.text=level.ToString();
         



    }
    public void Setexp(float exp) {
        expbar.fillAmount=exp;
    }
    public void SetSkill(int skill) {
        if (skill==1) {
            skill1.fillAmount=1;
        }
        else if (skill==2) {
            skill2.fillAmount=1;
        }
        else if (skill==3) {
            skill3.fillAmount=1;
        }
    }
}
