// ServantAttackState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantAttackState : ServantBaseState
{
    private Coroutine atkCo;

    public ServantAttackState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        float facing = fsm.CurrentFacing;              // +1 이면 우, -1 이면 좌
        fsm.Attack.SetDirection(facing);

        // 애니메이션 재생
        PlayDirectionalAnim("Attack");

        // 콜라이더 처리 시작
        if (PhotonNetwork.IsMasterClient)
            atkCo = fsm.StartCoroutine(AttackRoutine());
    }

    public override void Execute()
    {
        // 지속적으로 방향 보정 (optional)
        if (!PhotonNetwork.IsMasterClient) return;
        RefreshFacingToTarget();
        PlayDirectionalAnim("Attack");
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(fsm.attackDuration * 0.3f);

        fsm.Attack.SetDirection(fsm.CurrentFacing);
        fsm.Attack.EnableAttack();

        yield return new WaitForSeconds(fsm.attackDuration * 0.4f);

        fsm.Attack.DisableAttack();

        yield return new WaitForSeconds(fsm.attackDuration * 0.3f);
        fsm.TransitionToState(typeof(ServantAttackCoolState));
    }

    public override void Exit()
    {
        // 혹시 남아있으면 꺼주기
        if (atkCo != null)
            fsm.StopCoroutine(atkCo);
        fsm.Attack.DisableAttack();
    }
}
