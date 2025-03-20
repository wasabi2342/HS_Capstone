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
    private float originalSpeed; // ���� �ӵ� ����

    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed; // ���� �ӵ� ����
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
        agent.speed = originalSpeed; // �߰� ���� �� ���� �ӵ��� ����
    }
}
