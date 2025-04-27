using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster Data")]
public class EnemyStatus : ScriptableObject
{
    [Header("Info")]
    public int id;
    public enum Type { NORMAL, ELITE, BOSS }
    public Type type;
    public new string name;
    public float damage;
    public float hp;
    public float headOffset;

    [Header("Shield (0 = no)")]
    public float maxShield;
    public float hitRecoverTime;

    [Header("Attack")]
    public float waitCool;        // 공격 준비 시간
    public float attackCool;      // 공격 쿨타임
    public float attackRange;     // 공격 거리
    public float animDuration;    // 공격 애니메이션 길이

    [Header("AI Settings")]
    public float detectRange = 10f;     // 플레이어 탐지 거리
    public float wanderSpeed = 2f;      // 배회 속도
    public float chaseSpeed = 5f;       // 추적 속도
}
