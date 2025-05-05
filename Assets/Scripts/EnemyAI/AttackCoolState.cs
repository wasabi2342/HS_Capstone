using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackCoolState : BaseState
{
    Coroutine coolCo;
    public AttackCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            coolCo = fsm.StartCoroutine(CoolTime());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator CoolTime()
    {
        yield return new WaitForSeconds(status.attackCoolTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        bool immediate =
            fsm.LastAttackSuccessful &&
            fsm.Target &&
            (fsm.Target.position - transform.position).sqrMagnitude <=
            status.attackRange * status.attackRange;

        fsm.TransitionToState(immediate ? typeof(WaitCoolState) : typeof(WanderState));
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
