// ServantAttackCoolState.cs  (���� ���� ��Ÿ��)
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantAttackCoolState : ServantBaseState
{
    Coroutine coolCo;

    public ServantAttackCoolState(ServantFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        agent.isStopped = true;
        PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            coolCo = fsm.StartCoroutine(CoolRoutine());
    }

    public override void Execute() { /* Idle ���� ���� */ }

    IEnumerator CoolRoutine()
    {
        yield return new WaitForSeconds(fsm.attackCoolTime);
        // ��Ÿ� ���� ���(���� ����), �ƴϸ� Wander
        bool inRange = !fsm.Agent.pathPending
                       && fsm.Agent.remainingDistance <= fsm.attackRange;
        fsm.TransitionToState(inRange
            ? typeof(ServantWaitCoolState)
            : typeof(ServantWanderState));
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
