using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

/// <summary>
/// 타깃과 정면은 맞췄지만 z 오차가 클 때
/// 좌/우로 우회해 z축을 정렬한 뒤 WaitCoolState 로 복귀.
/// </summary>
public class DetourState : BaseState
{
    /* ───────── 튜닝 ───────── */
    const float REP_INT = 0.2f;   // 경로 재설정 주기
    const float MAX_TIME = 1.2f;    // 우회 시도 시간 한계
    const float END_DIST = 0.05f;   // “도달” 판정 거리
    const float NEAR_FACTOR = 1.3f;    // 근접 판정 배수(attackRange × n)
    const float LATERAL_MUL = 2f;      // 좌/우로 비켜갈 배수

    /* ───────── 상태 ───────── */
    Vector3 detourPos;
    float timer, repT;

    public DetourState(EnemyFSM f) : base(f) { }

    // ───────────────────────── Enter ─────────────────────────
    public override void Enter()
    {
        float side = Mathf.Sign(fsm.CurrentFacing);     // -1 좌 / +1 우
        float atkR = status.attackRange;

        detourPos = fsm.Target.position;
        detourPos.x += side * atkR * LATERAL_MUL;     // 옆으로 2칸
        detourPos.y = transform.position.y;

        if (agent)
        {
            SetAgentStopped(false);
            agent.stoppingDistance = 0f;
            agent.SetDestination(detourPos);
        }

        timer = repT = 0f;
        fsm.PlayDirectionalAnim("Chase");
    }

    // ───────────────────────── Execute ─────────────────────────
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 0) 타깃 유효성 검사 */
        if (fsm.Target == null || !fsm.Target.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(WanderState));
            return;
        }

        /* 1) 근접 여부 재확인 – 멀어졌으면 즉시 Chase */
        bool near = fsm.GetTarget2DDistSq() <=
                    status.attackRange * status.attackRange * NEAR_FACTOR * NEAR_FACTOR;
        if (!near)
        {
            fsm.TransitionToState(typeof(ChaseState));
            return;
        }

        /* 2) 경로 주기적 갱신 (타깃이 움직이면 z 다시 맞춤) */
        repT += Time.deltaTime;
        if (repT >= REP_INT && !agent.pathPending)
        {
            detourPos.z = fsm.Target.position.z;
            agent.SetDestination(detourPos);
            repT = 0f;
        }

        /* 3) 우회 지점 도달 또는 시간 초과 처리 */
        timer += Time.deltaTime;
        bool reached = !agent.pathPending &&
                       agent.remainingDistance <= END_DIST;

        if (reached || timer > MAX_TIME)
        {
            // 도달 직후 z-정렬 재확인
            if (fsm.IsAlignedAndInRange())
                fsm.TransitionToState(typeof(WaitCoolState));
            else
                fsm.TransitionToState(typeof(ChaseState));
            return;
        }

        /* 4) 방향 & 애니메이션 유지 */
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Chase");
    }

    // ───────────────────────── Exit ─────────────────────────
    public override void Exit() => SetAgentStopped(true);
}
