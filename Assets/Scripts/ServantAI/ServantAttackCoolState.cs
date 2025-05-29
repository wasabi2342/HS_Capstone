// ServantAttackCoolState.cs  (공격 직후 쿨타임)
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

    public override void Execute() { /* Idle 상태 유지 */ }

    IEnumerator CoolRoutine()
    {
        Debug.Log($"AttackCoolTime 사용 중: {fsm.attackCoolTime}");
        yield return new WaitForSeconds(fsm.attackCoolTime);
        fsm.TransitionToState(typeof(ServantChaseState));
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
