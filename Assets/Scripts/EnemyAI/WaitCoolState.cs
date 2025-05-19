using UnityEngine;
using System.Collections;
using Photon.Pun;

public class WaitCoolState : BaseState
{
    Coroutine waitCo;
    public WaitCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.Anim.speed = 1f;  // 애니메이션 속도 초기화
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            waitCo = fsm.StartCoroutine(WaitRoutine());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!fsm.IsAlignedAndInRange())
        {
            if (fsm.debugMode) Debug.Log("[WaitCool] ▶ 정렬 해제 → Chase", fsm);
            fsm.Agent.isStopped=false;                 // Agent 재시동
            fsm.TransitionToState(typeof(ChaseState));
            return;
        }
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(status.waitCoolTime);

        bool canAtk = fsm.IsAlignedAndInRange();
        if (fsm.debugMode)
            Debug.Log($"[WaitCool] time={status.waitCoolTime}s, "
                    + $"dist={Mathf.Sqrt(fsm.GetTarget2DDistSq()):0.00}, "
                    + $"result={(canAtk ? "Attack" : "Chase")}", fsm);

        fsm.TransitionToState(canAtk ? typeof(AttackState) : typeof(ChaseState));
    }


    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(waitCo);
    }
}
