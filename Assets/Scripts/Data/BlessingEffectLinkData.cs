using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "BlessingEffectLinkData", menuName = "Scriptable Objects/BlessingEffectLinkData")]
public class BlessingEffectLinkData : ScriptableObject
{
    public int ID;
    public int Blessing_ID;
    public int SpecialEffect_ID;
    public float Value;
    public float Duration;
}
