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

    private NavMeshAgent _agent;
    private int _currentIndex = 0;
    private int _direction = 1; // 1: forward, -1: backward
    private float _arrivalThreshold = 1.0f;

    protected override Status OnStart()
    {
        if (Self.Value == null || PatrolPoints.Value == null || PatrolPoints.Value.Count < 2)
        {
            Debug.LogWarning("PatrolAction: At least two patrol points are required.");
            return Status.Failure;
        }

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogWarning("PatrolAction: No NavMeshAgent found on Self.");
            return Status.Failure;
        }

        _agent.SetDestination(PatrolPoints.Value[_currentIndex].transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null || PatrolPoints.Value.Count < 2)
        {
            return Status.Failure;
        }

        if (!_agent.pathPending && _agent.remainingDistance < _arrivalThreshold)
        {
            // 이동 방향을 반대로 변경하여 1 <-> 2 반복
            _currentIndex += _direction;
            if (_currentIndex >= PatrolPoints.Value.Count - 1 || _currentIndex <= 0)
            {
                _direction *= -1; // 방향 반전
            }

            Vector3 nextPosition = PatrolPoints.Value[_currentIndex].transform.position;
            nextPosition.y = _agent.transform.position.y;
            _agent.SetDestination(nextPosition);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null)
        {
            _agent.ResetPath();
        }
    }
}