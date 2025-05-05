// ========================= WaitCoolState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class WaitCoolState : BaseState
{
    private Coroutine waitCo;
    public WaitCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            waitCo = fsm.StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(status.waitCoolTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        if (fsm.Target && (fsm.Target.position - transform.position).sqrMagnitude <= status.attackRange * status.attackRange)
            fsm.TransitionToState(typeof(AttackState));
        else
            fsm.TransitionToState(typeof(ChaseState));
    }

    public override void Execute() 
    {
        fsm.PlayDirectionalAnim("Idle");
    }

    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(waitCo);
    }

}
