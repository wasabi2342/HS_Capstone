using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "BasicAttackComboData", menuName = "Scriptable Objects/BasicAttackComboData")]
public class BasicAttackComboData : ScriptableObject
{
    public int ID;
    public int Character;
    public int Combo_Index;
    public float Damage;
}
