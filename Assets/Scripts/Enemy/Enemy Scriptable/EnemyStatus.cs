using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster Data")]
public class EnemyStatus : ScriptableObject
{
    [Header("Info")]
    public int id; //몬스터 ID
    public enum Type
    {
        NORMAL,
        ELITE,
        BOSS
    }
    public Type type; //몬스터 타입
    public new string name; //이름
    public int dmg; //데미지
    public int hp; //체력
    public float speed; //속력

    [Header("Attack")]
    public float waitCool; //공격 준비 시간
    public float attackCool; //쿨타임
}
