using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public abstract class BaseState : IState
{
    protected readonly EnemyFSM fsm;
    protected readonly NavMeshAgent agent;
    protected readonly EnemyStatusSO status;
    protected readonly Animator animator;
    protected readonly Transform transform;

    protected BaseState(EnemyFSM f)
    {
        fsm = f;
        agent = f.Agent;
        status = f.EnemyStatusRef;
        animator = f.Anim;
        transform = f.transform;
    }

    protected bool CanControlAgent =>
        PhotonNetwork.IsMasterClient && agent && agent.enabled;

    protected void SetAgentStopped(bool stop)
    {
        if (CanControlAgent) agent.isStopped = stop;
    }

    /* ───────── 방향 갱신 메서드 ───────── */
    protected void RefreshFacingToTarget()
    {
        if (fsm.Target == null) return;

        float dx = fsm.Target.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;        // 거의 정면은 무시

        fsm.ForceFacing(dx);                      // + → Right, – → Left
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
