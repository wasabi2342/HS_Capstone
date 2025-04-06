using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "SpecialEffectData", menuName = "Scriptable Objects/SpecialEffectData")]
public class SpecialEffectData : ScriptableObject
{
    public int ID;
    public string EffectName;
    public string Description;
}
