using UnityEngine;
using Photon.Pun;

public class AttackCoolState : BaseState
{
    float timer;

    public AttackCoolState(EnemyFSM f) : base(f) { }

    /* ── 들어올 때 ────────────────────────────── */
    public override void Enter()
    {
        timer = 0f;
        SetAgentStopped(true);                 // 제자리 대기
        fsm.PlayDirectionalAnim("Idle");
    }

    /* ── 매 프레임 ────────────────────────────── */
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 1) 쿨타임 카운트 */
        timer += Time.deltaTime;
        if (timer < status.attackCoolTime)     // ★ 쿨타임 끝날 때까지 아무것도 안 함
        {
            RefreshFacingToTarget();           // 바라보는 방향만 유지
            return;
        }
        bool aligned = fsm.IsAlignedAndInRange();
        /* 2) 쿨타임이 끝난 뒤 분기 */
        if (aligned)       // 탐지 반경 안 → 추적 재개
        {
            SetAgentStopped(true);
            fsm.TransitionToState(typeof(WaitCoolState));
        }
        else                                   // 멀어짐 → 순찰/귀환
        {
            SetAgentStopped(false);
            fsm.TransitionToState(typeof(WanderState));  // 필요하면 ReturnState
        }
    }

    /* ── 나갈 때 ────────────────────────────── */
    public override void Exit()
    {
        SetAgentStopped(false);                // 다음 상태가 이동 가능하도록
    }
}
