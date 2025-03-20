using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Self] patrols along [PatrolPoints] in order", category: "Action", id: "e9d993a828ed8219e61fd0c76a825437")]
public partial class PatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1; // 1: forward, -1: backward
    private float arrivalThreshold = 1.0f;

    protected override Status OnStart()
    {
        if (Self.Value == null || PatrolPoints.Value == null || PatrolPoints.Value.Count < 2)
        {
            Debug.LogWarning("PatrolAction: At least two patrol points are required.");
            return Status.Failure;
        }

        agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning("PatrolAction: No NavMeshAgent found on Self.");
            return Status.Failure;
        }

        agent.SetDestination(PatrolPoints.Value[currentIndex].transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (agent == null || PatrolPoints.Value.Count < 2)
        {
            return Status.Failure;
        }

        if (!agent.pathPending && agent.remainingDistance < arrivalThreshold)
        {
            // 이동 방향을 반대로 변경하여 1 <-> 2 반복
            currentIndex += direction;
            if (currentIndex >= PatrolPoints.Value.Count - 1 || currentIndex <= 0)
            {
                direction *= -1; // 방향 반전
            }

            Vector3 nextPosition = PatrolPoints.Value[currentIndex].transform.position;
            nextPosition.y = agent.transform.position.y;
            agent.SetDestination(nextPosition);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (agent != null)
        {
            agent.ResetPath();
        }
    }
}