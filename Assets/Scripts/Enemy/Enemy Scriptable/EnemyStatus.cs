using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster Data")]
public class EnemyStatus : ScriptableObject
{
    [Header("Info")]
    public int id; //���� ID
    public enum Type
    {
        NORMAL,
        ELITE,
        BOSS
    }
    public Type type; //���� Ÿ��
    public new string name; //�̸�
    public int dmg; //������
    public int hp; //ü��
    public float speed; //�ӷ�

    [Header("Attack")]
    public float waitCool; //���� �غ� �ð�
    public float attackCool; //��Ÿ��
}
