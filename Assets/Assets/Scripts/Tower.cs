using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TowerType
{
    Normal,//근접
    Sniper,//원거리
    Magic,//마법
    Support,//지원
}


[CreateAssetMenu(fileName = "Tower", menuName = "Scriptable Objects/Tower")]
public class Tower : ScriptableObject
{
    [Header("유닛 능력치")]
    public int hp;
    public int attack;

    public float moveSpeed;

    [Header("공격 설정")]
    public float attackRange = 3f;        // 공격 범위
    public float attackSpeed = 1f;        // 공격 속도 (초당 공격 횟수)
    public GameObject bulletPrefab;       // 총알 프리팹
    public float bulletSpeed = 5f;        // 총알 속도
    public TowerType towerType;

    [Header("유닛 외형")]
    public Sprite unitSprite;
    public Color unitColor = Color.white; // 유닛 색상 (기본값: 흰색)

    public int goldCost;
    public Sprite TowerSprite;
    public int spawnCooldown;

    public RuntimeAnimatorController animatorController;
    public List<Exhence> exhenceList = new List<Exhence>();
}
