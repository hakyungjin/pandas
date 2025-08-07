using UnityEngine;

public enum ExhenceType
{
    StatBoost,     // 능력치 강화
    NewAbility     // 새로운 능력 부여
}

[System.Serializable]
public class Exhence
{
    public string exhenceName;
    public ExhenceType type;

    // 능력치 보정
   public int hpBonus = 0;
    public int damageBonus = 0;
    public int moveSpeedBonus = 0;

    
    public float attackRangeBonus = 0f;
    public float attackSpeedBonus = 0f;
    public int attackBonus = 0;
    // 특수 능력 (type == NewAbility 일 때 사용)
    public string abilityKey;  // ex: "FireBreath", "Dash", "FreezeAura"
    
    
}

