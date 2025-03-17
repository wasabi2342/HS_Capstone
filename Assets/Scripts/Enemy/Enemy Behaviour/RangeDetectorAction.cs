using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangeDetector", story: "Update Range [Detector] and assign [Player]", category: "Action", id: "e26e17d5d773ac9a34cabc8cbab5e547")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Player;


    protected override Status OnUpdate()
    {
        Player.Value = Detector.Value.UpdateDetector();
        return Detector.Value.UpdateDetector() == null ? Status.Failure : Status.Success;
    }

}

