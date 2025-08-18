using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Herostate : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI levelText;
    public Image hpbar;
    public Image expbar;
    public Image skill1;
    public Image skill2;
    public Image skill3;

    public Sprite Reviveicon;
    public Sprite normalicon;
    

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
    public void SetReviveIcon() {
        icon.sprite=Reviveicon;
        icon.transform.GetChild(0).gameObject.SetActive(false);
        icon.transform.GetChild(1).gameObject.SetActive(false);
    }
    public void SetNormalIcon() {
        icon.sprite=normalicon;
        icon.transform.GetChild(0).gameObject.SetActive(true);
        icon.transform.GetChild(1).gameObject.SetActive(true);
    }
}
