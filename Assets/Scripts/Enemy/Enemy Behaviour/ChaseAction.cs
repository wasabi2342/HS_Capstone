using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[Self] Navigate To [Player]", category: "Action", id: "57cad14ab4ddcebfc814630b110ddc44")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    private NavMeshAgent agent;
    private float originalSpeed; // 기존 속도 저장

    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed; // 기존 속도 저장
        agent.speed = 5f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Player.Value == null)
        {
            return Status.Failure;
        }

        agent.SetDestination(Player.Value.transform.position);
        return Status.Running;
    }

    protected override void OnEnd()
    {
        agent.speed = originalSpeed; // 추격 종료 후 원래 속도로 복귀
    }
}
