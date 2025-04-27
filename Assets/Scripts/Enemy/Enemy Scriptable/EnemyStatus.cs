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
    public float waitCool;        // ���� �غ� �ð�
    public float attackCool;      // ���� ��Ÿ��
    public float attackRange;     // ���� �Ÿ�
    public float animDuration;    // ���� �ִϸ��̼� ����

    [Header("AI Settings")]
    public float detectRange = 10f;     // �÷��̾� Ž�� �Ÿ�
    public float wanderSpeed = 2f;      // ��ȸ �ӵ�
    public float chaseSpeed = 5f;       // ���� �ӵ�
}
