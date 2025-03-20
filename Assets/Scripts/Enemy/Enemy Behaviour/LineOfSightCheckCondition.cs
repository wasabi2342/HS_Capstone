using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight Check", story: "Check [Player] With Line Of Sight [detector]", category: "Conditions", id: "57856978a900b3d44affc2938c16db36")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.PerformDetection(Player.Value) != null;
    }

}
