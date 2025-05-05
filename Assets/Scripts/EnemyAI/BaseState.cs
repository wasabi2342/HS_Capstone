// ========================= BaseState.cs
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseState : IState
{
    protected readonly EnemyFSM fsm;
    protected readonly NavMeshAgent agent;
    protected readonly EnemyStatusSO status;
    protected readonly Animator animator;
    protected readonly Transform transform;

    protected BaseState(EnemyFSM fsmController)
    {
        fsm = fsmController;
        agent = fsmController.Agent;
        status = fsmController.EnemyStatusRef;
        animator = fsmController.Anim;
        transform = fsmController.transform;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
