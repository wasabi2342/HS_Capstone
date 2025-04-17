using UnityEngine;
using System;
public enum SpecialEffectType { None, Dot, Slow, Bind }

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

    public SpecialEffectType debuffType = SpecialEffectType.None;
    public float debuffDuration = 0f;
    public float debuffValue = 0f;
}
