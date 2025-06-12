using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "R_AttackComboData", menuName = "Scriptable Objects/R_AttackComboData")]
public class R_AttackComboData : ScriptableObject
{
    public int ID;
    public int Character;
    public int Combo_Index;
    public float Damage;
}
