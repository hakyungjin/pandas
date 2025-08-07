using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstallManager : MonoBehaviour
{
    [Header("설치할 유닛 데이터 및 프리팹")]
    public Tower unitData;          // ScriptableObject 데이터
    public GameObject unitPrefab;   // 프리팹 (스프라이트 출력 포함)

    [Header("레이어 마스크")]
    public LayerMask installLayer;  // 설치 가능한 영역만 클릭되도록

    // 드래그&드롭 방식에서는 Update 함수가 필요 없습니다.
    // 필요하다면 Uiunit에 unitData/unitPrefab을 할당하는 역할만 남깁니다.
}
