using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Herostate : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI levelText;
    public Image hpbar;
    public Image expbar;
    public GameObject skill1;
    public GameObject skill2;
    public GameObject skill3;

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
    public void SetSkill1Cooldown() {
        skill1.transform.GetChild(0).gameObject.SetActive(true);
        skill1.transform.GetChild(1).gameObject.SetActive(false);
    }
    public void SetSkill2Cooldown() {
        skill2.transform.GetChild(0).gameObject.SetActive(true);
        skill2.transform.GetChild(1).gameObject.SetActive(false);
    }
    public void SetSkill3Cooldown() {
        skill3.transform.GetChild(0).gameObject.SetActive(true);
        skill3.transform.GetChild(1).gameObject.SetActive(false);
    }
    public void SetSkill1CooldownEnd() {
        skill1.transform.GetChild(0).gameObject.SetActive(false);
        skill1.transform.GetChild(1).gameObject.SetActive(true);
    }
    public void SetSkill2CooldownEnd() {
        skill2.transform.GetChild(0).gameObject.SetActive(false);
        skill2.transform.GetChild(1).gameObject.SetActive(true);
    }
    public void SetSkill3CooldownEnd() {
        skill3.transform.GetChild(0).gameObject.SetActive(false);
        skill3.transform.GetChild(1).gameObject.SetActive(true);
    }
}
