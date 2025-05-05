using UnityEngine;
using System.Collections;
using Photon.Pun;

public class HitState : BaseState
{
    Coroutine stunCo;
    public HitState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.PlayDirectionalAnim("Hit");

        if (fsm.currentHP <= 0)
        {
            fsm.TransitionToState(typeof(DeadState)); return;
        }

        if (PhotonNetwork.IsMasterClient)
            stunCo = fsm.StartCoroutine(Stun());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Hit");
    }

    IEnumerator Stun()
    {
        yield return new WaitForSeconds(status.hitStunTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        fsm.TransitionToState(fsm.Target ? typeof(ChaseState) : typeof(WanderState));
    }

    public override void Exit()
    {
        if (stunCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(stunCo);
    }
}
