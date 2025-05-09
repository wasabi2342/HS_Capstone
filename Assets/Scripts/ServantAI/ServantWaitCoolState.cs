// ServantWaitCoolState.cs  (공격 직전 대기)
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantWaitCoolState : ServantBaseState
{
    Coroutine waitCo;

    public ServantWaitCoolState(ServantFSM f) : base(f) { }

    public override void Enter()
    {
        // facing 갱신, 멈추고 Idle 애니
        RefreshFacingToTarget();
        agent.isStopped = true;
        PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            waitCo = fsm.StartCoroutine(WaitRoutine());
    }

    public override void Execute()
    {
        // 적이 사거리 밖으로 나가면 곧장 추적
        if (!PhotonNetwork.IsMasterClient) return;
        if (fsm.Agent.remainingDistance > fsm.attackRange)
            fsm.TransitionToState(typeof(ServantChaseState));
    }

    IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(fsm.waitCoolTime);
        // 사거리 내면 Attack, 아니면 Chase
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
