using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wander", story: "[Self] Navigate To WanderPosition", category: "Action", id: "83a3164f8afd5f93eda6f1125b2d14b1")]
public partial class WanderAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private NavMeshAgent agent;
    private Vector3 wanderPosition;
    private float currentWanderTime = 0f;
    private float maxWanderTime = 5f;

    protected override Status OnStart()
    {
        int jitterMin = 0;
        int jitterMax = 360;
        float wanderRadius = UnityEngine.Random.Range(5, 10);
        int wanderJitter = UnityEngine.Random.Range(jitterMin, jitterMax);

        //목표 위치 = 자신(self)의 위치 + 각도(wanderJitter)에 해당하는 반지름 (wanderRadius) 크기의 원의 둘레 위치
        wanderPosition = Self.Value.transform.position + Utills.GetPositionFromAngle(wanderRadius, wanderJitter);
        agent = Self.Value.GetComponent<NavMeshAgent>();
        agent.SetDestination(wanderPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if((wanderPosition-Self.Value.transform.position).sqrMagnitude < 0.1f || Time.time - currentWanderTime > maxWanderTime)
        {
            return Status.Success;
        }
        return Status.Running;
    }
}

