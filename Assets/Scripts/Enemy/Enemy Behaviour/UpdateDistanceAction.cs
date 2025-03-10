using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateDistance", story: "Update [Self] and [Player] [CurrentDistance]", category: "Action", id: "f03cfc4d63eccffc4d0ef6e05046d043")]
public partial class UpdateDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;

    protected override Status OnUpdate()
    {
        CurrentDistance.Value = Vector3.Distance(Self.Value.transform.position, Player.Value.transform.position);
        return Status.Success;
    }
}

