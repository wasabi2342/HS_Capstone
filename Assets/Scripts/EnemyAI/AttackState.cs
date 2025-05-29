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
        if (fsm.Agent) fsm.Agent.ResetPath();
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
        float windUpRate = fsm.AttackComponent is IMonsterAttack atk
                           ? Mathf.Clamp01(atk.WindUpRate)
                           : 0.5f;                         // 안전값

        float windUp = status.attackDuration * windUpRate;
        float follow = status.attackDuration - windUp;

        yield return new WaitForSeconds(windUp);           // 개별 몬스터 산출

        fsm.AttackComponent?.Attack(fsm.Target);           // 돌진·투사체 등 실제 실행

        yield return new WaitForSeconds(follow);           // 후딜

        fsm.TransitionToState(typeof(AttackCoolState));
    }

    public override void Exit()
    {
        if (atkCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(atkCo);
    }
}