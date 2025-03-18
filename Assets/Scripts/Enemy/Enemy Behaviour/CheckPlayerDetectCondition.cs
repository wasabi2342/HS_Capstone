using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckPlayerDetect", story: "Compare values of [CurrentDistance] and [ChaseDistance]", category: "Conditions", id: "553a0e10ba6e81ae666ec6d04f812c6f")]
public partial class CheckPlayerDetectCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<float> ChaseDistance;

    public override bool IsTrue()
    {
        if (CurrentDistance.Value <= ChaseDistance.Value)
        {
            return true;
        }
        return false;
    }

}
