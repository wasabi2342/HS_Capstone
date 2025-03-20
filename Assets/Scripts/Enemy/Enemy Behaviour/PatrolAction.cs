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
    private float arrivalThreshold = 1.0f;
    private float originalSpeed; // 기존 속도 저장
    private Vector3 patrolOffset; // 몬스터 개별 오프셋

    protected override Status OnStart()
    {
        if (Self.Value == null || PatrolPoints.Value == null || PatrolPoints.Value.Count < 2)
        {
            Debug.LogWarning("PatrolAction: Not enough patrol points to patrol.");
            return Status.Failure;
        }

        agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning("PatrolAction: No NavMeshAgent found on Self.");
            return Status.Failure;
        }

        if (originalSpeed == 0)
        {
            originalSpeed = agent.speed; // 처음 실행 시 한 번만 저장
        }

        agent.isStopped = false;
        agent.speed = originalSpeed;

        patrolOffset = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));

        MoveToNextPatrolPoint();

        Debug.Log($"PatrolAction: Starting patrol at point {currentIndex + 1}/{PatrolPoints.Value.Count}, Speed: {agent.speed}");

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
            MoveToNextPatrolPoint();
        }

        return Status.Running;
    }

    private void MoveToNextPatrolPoint()
    {
        currentIndex = (currentIndex + 1) % PatrolPoints.Value.Count;

        Vector3 basePosition = PatrolPoints.Value[currentIndex].transform.position;
        Vector3 nextPosition = basePosition + patrolOffset;
        nextPosition.y = agent.transform.position.y;

        agent.SetDestination(nextPosition);
        Debug.Log($"PatrolAction: Moving to patrol point {currentIndex + 1}/{PatrolPoints.Value.Count}, Offset: {patrolOffset}, Speed: {agent.speed}");
    }

    protected override void OnEnd()
    {
        if (agent != null)
        {
            agent.ResetPath();
            Debug.Log("PatrolAction: Patrol stopped.");
        }
    }
}
