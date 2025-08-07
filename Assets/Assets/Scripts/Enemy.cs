using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
    [Header("유닛 능력치")]
    public int hp;
    public int attack;

    [Header("유닛 외형")]
    public Sprite unitSprite;
    public Color unitColor = Color.white; // 유닛 색상 (기본값: 흰색)
    public GameObject hpBarPrefab; // HP바 프리팹

    public RuntimeAnimatorController animatorController;
}
