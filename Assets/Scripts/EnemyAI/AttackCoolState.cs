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
        // 공격 후 쿨타임 대기
        yield return new WaitForSeconds(status.attackCoolTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        // 플레이어가 아직 정렬(거리 & Z) 조건을 만족하면 Wander로 가지 않고 바로 WaitCoolState로 전환
        bool immediate = fsm.Target && fsm.IsAlignedAndInRange();

        fsm.TransitionToState(immediate ? typeof(WaitCoolState) : typeof(WanderState));
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
