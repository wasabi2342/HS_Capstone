using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

/// <summary>
/// 자연스러운 대각선 추적 + 정렬 실패 타임아웃 + 자동 Detour 로직 통합 버전
/// </summary>
public class ChaseState : BaseState
{
    /* ───────── 튜닝 파라미터 ───────── */
    const float REP_PATH_INTERVAL = 0.20f;   // 경로 재계산 주기
    const float DETOUR_LATERAL = 1.5f;      // 옆으로 비켜갈 배율 (attackRange × n)
    const float NO_ALIGN_TIME = 1.5f;    // n 초 동안 WaitCool에 못 들어가면 Detour
    const float NEAR_FACTOR = 1.25f;   // “근접” 판정 배율(attackRange × n)

    /* ───────── 상태 변수 ───────── */
    bool detouring;           
    float repathT;             // 경로 재계산 타이머
    float chaseTimer;          // 최대 추적 시간
    float noAlignTimer;        

    public ChaseState(EnemyFSM f) : base(f) { }

    // ─────────────────────────── Enter ───────────────────────────
    public override void Enter()
    {
        detouring = false;
        repathT = 0f;
        chaseTimer = 0f;
        noAlignTimer = 0f;

        if (agent)
        {
            SetAgentStopped(false);
            agent.speed = status.moveSpeed *
                                     (status.chaseSpeedMultiplier > 0 ? status.chaseSpeedMultiplier : 1f);
            agent.stoppingDistance = status.attackRange;
            agent.autoBraking = true;    // 부드러운 감속
        }

        fsm.PlayDirectionalAnim("Chase");
    }

    // ────────────────────────── Execute ──────────────────────────
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;     // AI 처리 책임

        /* 0) 타깃 유효성 */
        if (fsm.Target == null || !fsm.Target.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(WanderState));
            return;
        }

        /* 1) 최대 거리‧시간 초과 */
        chaseTimer += Time.deltaTime;
        if (chaseTimer > status.maxChaseTime ||
            (fsm.Target.position - fsm.spawnPosition).sqrMagnitude >
            status.maxChaseDistance * status.maxChaseDistance)
        {
            fsm.Target = null;
            fsm.TransitionToState(typeof(ReturnState));
            return;
        }

        /* 2) 위치‧정렬 데이터 계산 */
        float xDiff = fsm.Target.position.x - transform.position.x;
        float xAbs = Mathf.Abs(xDiff);
        float zDiff = fsm.GetZDiffAbs();
        float tol = fsm.zAlignTolerance;
        float atkR = status.attackRange;

        bool aligned = fsm.IsAlignedAndInRange();
        bool near = fsm.GetTarget2DDistSq() <= atkR * atkR * NEAR_FACTOR * NEAR_FACTOR;

        /* 3) ───── 정렬 성공 → WaitCoolState 전환 ───── */
        if (aligned)
        {
            agent.isStopped = true;
            fsm.TransitionToState(typeof(WaitCoolState));
            return;
        }

        /* 4) ───── WaitCool 미진입 타이머 ───── */
        if (near && !detouring)      // 근접 상태에서만 카운트
            noAlignTimer += Time.deltaTime;
        else
            noAlignTimer = 0f;

        if (noAlignTimer >= NO_ALIGN_TIME)
        {
            if (fsm.debugMode)
                Debug.Log($"[Chase] {NO_ALIGN_TIME}s 내 정렬 실패 → DetourState", fsm);
            fsm.TransitionToState(typeof(DetourState));
            return;
        }

        /* 5) ───── Detour 진입 조건 ─────
               - 거의 정면(xAbs ≤ tol)
               - 근접
               - z 오차가 tol 초과 */
        if (!detouring &&
            zDiff > tol &&
            xAbs <= tol &&
            near)
        {
            float side = Mathf.Sign(fsm.CurrentFacing); // -1 좌 / +1 우
            Vector3 det = fsm.Target.position;
            det.x += side * atkR * DETOUR_LATERAL;
            det.y = transform.position.y;

            agent.stoppingDistance = 0f;
            agent.SetDestination(det);
            detouring = true;
            return;
        }

        /* 6) ───── Detour 진행 중 판정 ───── */
        if (detouring)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                detouring = false;
                agent.stoppingDistance = atkR;   // 원래 값 복구
            }
            else
            {
                // Detour 중엔 다른 로직 생략
                RefreshFacingToTarget();
                fsm.PlayDirectionalAnim("Chase");
                return;
            }
        }

        /* 7) ───── 평상시 추적 경로 재계산 ───── */
        repathT += Time.deltaTime;
        if (repathT >= REP_PATH_INTERVAL && !agent.pathPending)
        {
            Vector3 dest = fsm.Target.position;
            dest.y = transform.position.y;
            agent.SetDestination(dest);
            repathT = 0f;
        }

        /* 8) ───── 방향 & 애니메이션 동기화 ───── */
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Chase");
    }

    // ─────────────────────────── Exit ───────────────────────────
    public override void Exit()
    {
        if (agent) agent.autoBraking = false;
        detouring = false;
    }
}
