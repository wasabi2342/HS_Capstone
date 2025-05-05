using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackState : BaseState
{
    Coroutine atkCo;
    public AttackState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);

        if (PhotonNetwork.IsMasterClient)
        {
            fsm.PlayDirectionalAnim("Attack");
            atkCo = fsm.StartCoroutine(AttackRoutine());
        }
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Attack");
    }

    IEnumerator AttackRoutine()
    {
        float half = status.attackDuration * 0.5f;
        yield return new WaitForSeconds(half);

        bool hit = fsm.IsAlignedAndInRange();
        fsm.LastAttackSuccessful = hit;

        if (fsm.debugMode)
            Debug.Log($"[Attack] hit={hit}, dist={Mathf.Sqrt(fsm.GetTarget2DDistSq()):0.00}", fsm);

        yield return new WaitForSeconds(half);
        fsm.TransitionToState(typeof(AttackCoolState));
    }


    public override void Exit()
    {
        if (atkCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(atkCo);
    }
}
