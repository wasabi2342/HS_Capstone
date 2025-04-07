using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public int ID;
    public int Devil;
    public int Bind_Key;
    public int Character;
    public string Blessing_name;
    public string Bless_Discript;
    public float AttackDamageCoefficient; 
    public float AbilityPowerCoefficient;
    public float Cooldown;
    public int Stack;
}
