// ========================= AttackState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackState : BaseState
{
    private Coroutine atkCo;
    public AttackState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;

        if (PhotonNetwork.IsMasterClient)
        {
            fsm.photonView.RPC("PlayAttackAnimRPC", RpcTarget.All);
            atkCo = fsm.StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(status.attackDuration * .5f);

        bool hit = false;
        if (fsm.Target && (fsm.Target.position - transform.position).sqrMagnitude <= status.attackRange * status.attackRange)
        {
            // 실제 데미지 적용 로직, 콜 RPC 등
            hit = true;
        }
        fsm.LastAttackSuccessful = hit;

        yield return new WaitForSeconds(status.attackDuration * .5f);
        fsm.TransitionToState(typeof(AttackCoolState));
    }

    public override void Execute() 
    {
        fsm.PlayDirectionalAnim("Attack");
    }

    public override void Exit()
    {
        if (atkCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(atkCo);
    }
}
