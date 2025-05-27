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
        fsm.Anim.speed = 1f;
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
        {
            if (waitCo != null) fsm.StopCoroutine(waitCo);   // 중복 방지
            waitCo = fsm.StartCoroutine(WaitRoutine());
        }
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* z-축 틀어졌거나 사정권 벗어남 → Chase */
        float zAbs = Mathf.Abs(transform.position.z - fsm.Target.position.z);
        if (zAbs > fsm.TolOutCache || !fsm.IsTargetInAttackRange())
        {
            if (fsm.debugMode) Debug.Log("[WaitCool] ▶ 정렬 해제 → Chase", fsm);
            SetAgentStopped(false);
            fsm.TransitionToState(typeof(ChaseState));
            return;
        }

        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(status.waitCoolTime);

        bool aligned = fsm.IsAlignedAndInRange();        // 정렬 + 사정거리
        bool canAtk = aligned /* && fsm.CanAttackNow() */; // 필요하면 조건 추가

        if (fsm.debugMode)
            Debug.Log($"[WaitCool] {status.waitCoolTime:F1}s → "
                    + (canAtk ? "Attack" : aligned ? "Detour" : "Chase"), fsm);

        if (canAtk)
            fsm.TransitionToState(typeof(AttackState));
        else if (aligned)          // 정렬 OK지만 공격 불가(쉴드 등) 시
            fsm.TransitionToState(typeof(DetourState));
        else
            fsm.TransitionToState(typeof(ChaseState));
    }

    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
        {
            fsm.StopCoroutine(waitCo);
            waitCo = null;
        }
    }
}
