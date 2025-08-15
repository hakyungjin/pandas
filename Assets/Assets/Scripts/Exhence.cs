using UnityEngine;

public enum ExhenceType
{
    unitTower,
    goldTower,
}

[System.Serializable]
public class Exhence
{  
    public string exhenceName;
    public ExhenceType type;

    // 능력치 보정
   public int hpBonus = 0;
    public int attackBonus = 0;
    public int moveSpeedBonus = 0;

    
    public float attackRangeBonus = 0f;
    public float attackSpeedBonus = 0f;

    public int goldcost = 0;

    public int additionalGold = 0;


    public string abilityKey;  // ex: "FireBreath", "Dash", "FreezeAura"
    
    
}

