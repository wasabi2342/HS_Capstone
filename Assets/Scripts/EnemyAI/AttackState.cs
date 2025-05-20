using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackState : BaseState
{
    Coroutine atkCo;
    public AttackState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        fsm.SelectNextAttackPattern();
        RefreshFacingToTarget();
        SetAgentStopped(true);
        // ─ 방향 넘겨 주기 ─
        fsm.AttackComponent.SetDirection(fsm.CurrentFacing);
        string clipBase = fsm.AttackComponent?.AnimKey ?? "Attack";
        fsm.PlayDirectionalAnim(clipBase);

        if (PhotonNetwork.IsMasterClient)
            atkCo = fsm.StartCoroutine(AttackRoutine());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        fsm.PlayDirectionalAnim("Attack");
    }

    IEnumerator AttackRoutine()
    {
        float half = status.attackDuration * 0.5f;

        /* 윈드업 구간 */
        yield return new WaitForSeconds(half);
        /* 실제 공격 활성화 */
        fsm.AttackComponent?.Attack(fsm.Target);
        /* 후딜 구간 */
        yield return new WaitForSeconds(half);

        /* 다음 상태로 */
        fsm.TransitionToState(typeof(AttackCoolState));
    }

    public override void Exit()
    {
        if (atkCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(atkCo);
    }
}