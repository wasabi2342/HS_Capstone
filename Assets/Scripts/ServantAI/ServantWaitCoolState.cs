// ServantWaitCoolState.cs  (���� ���� ���)
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantWaitCoolState : ServantBaseState
{
    Coroutine waitCo;

    public ServantWaitCoolState(ServantFSM f) : base(f) { }

    public override void Enter()
    {
        // facing ����, ���߰� Idle �ִ�
        RefreshFacingToTarget();
        agent.isStopped = true;
        PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            waitCo = fsm.StartCoroutine(WaitRoutine());
    }

    public override void Execute()
    {
        // ���� ��Ÿ� ������ ������ ���� ����
        if (!PhotonNetwork.IsMasterClient) return;
        if (fsm.Agent.remainingDistance > fsm.attackRange)
            fsm.TransitionToState(typeof(ServantChaseState));
    }

    IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(fsm.waitCoolTime);
        // ��Ÿ� ���� Attack, �ƴϸ� Chase
        bool inRange = !fsm.Agent.pathPending
                       && fsm.Agent.remainingDistance <= fsm.attackRange;
        fsm.TransitionToState(inRange
            ? typeof(ServantAttackState)
            : typeof(ServantChaseState));
    }

    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(waitCo);
    }
}
