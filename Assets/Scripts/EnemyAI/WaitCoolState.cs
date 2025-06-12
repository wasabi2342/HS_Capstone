using UnityEngine;
using System.Collections;
using Photon.Pun;

public class WaitCoolState : BaseState
{
    Coroutine waitCo;
    float waitTime;                     // 이번 사이클의 랜덤 대기 시간

    public WaitCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        // 이번에 사용할 대기 시간을 0.01~0.10 초 사이에서 결정
        waitTime = Random.Range(0.01f, 0.10f);

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

        /* z-축이 틀어졌거나 사정권 벗어남 → Chase 전환 */
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
        /* 랜덤 대기 */
        yield return new WaitForSeconds(waitTime);

        bool aligned = fsm.IsAlignedAndInRange();        // 정렬 + 사정거리
        bool canAtk = aligned; 

        if (fsm.debugMode)
            Debug.Log($"[WaitCool] {waitTime:F2}s → "
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
