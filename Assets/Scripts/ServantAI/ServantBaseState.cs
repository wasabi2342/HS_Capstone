// ServantBaseState.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public abstract class ServantBaseState : IState
{
    protected readonly ServantFSM fsm;
    protected readonly NavMeshAgent agent;
    protected readonly Animator anim;
    protected readonly Transform transform;

    protected ServantBaseState(ServantFSM s)
    {
        fsm = s;
        agent = s.Agent;
        anim = s.Anim;
        transform = s.transform;
    }

    protected bool CanControlAgent => agent != null;

    protected void SetAgentStopped(bool stop)
    {
        if (CanControlAgent) agent.isStopped = stop;
    }

    /// <summary>
    /// �־��� Ÿ��(Enemy) ��ġ�� �������� Facing�� �����մϴ�.
    /// </summary>
    protected void RefreshFacingToTarget()
    {
        if (fsm.TargetEnemy == null) return;
        float dx = fsm.TargetEnemy.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;
        fsm.ForceFacing(dx);
    }

    /// <summary>
    /// "Right_Action" �Ǵ� "Left_Action" ���·� �ִϸ��̼��� ����ϰ�, 
    /// ��Ʈ��ũ ����ȭ�� ���� RpcTarget.AllBuffered���� RPC�� �����ϴ�.
    /// </summary>
    protected void PlayDirectionalAnim(string action)
    {
        if (anim == null) return;

        string clip = (fsm.CurrentFacing >= 0 ? "Right_" : "Left_") + action;
        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(clip)) return;

        if (fsm.pv.IsMine)
            fsm.pv.RPC(nameof(ServantFSM.RPC_PlayClip), RpcTarget.AllBuffered, clip);

        anim.Play(clip, 0);
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
