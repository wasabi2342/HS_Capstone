// ========================= AttackCoolState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackCoolState : BaseState
{
    private Coroutine coolCo;
    public AttackCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            coolCo = fsm.StartCoroutine(CoolRoutine());
    }

    private IEnumerator CoolRoutine()
    {
        yield return new WaitForSeconds(status.attackCoolTime);

        if (!PhotonNetwork.IsMasterClient) yield break;

        bool canReAttack = fsm.LastAttackSuccessful &&
                           fsm.Target &&
                           (fsm.Target.position - transform.position).sqrMagnitude <= status.attackRange * status.attackRange;

        fsm.TransitionToState(canReAttack ? typeof(WaitCoolState) : typeof(WanderState));
    }

    public override void Execute() 
    {
        fsm.PlayDirectionalAnim("Idle");
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
