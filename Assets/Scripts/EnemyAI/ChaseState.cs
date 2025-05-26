using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

/// <summary>
/// 항상 플레이어 측면을 목표로 하는 추적 + 실패 시 Detour 전환
/// </summary>
public class ChaseState : BaseState
{
    /* ─── 튜닝 ─── */
    const float REP_INT = 0.18f;   // NavMesh 경로 재계산 주기
    const float SIDE_OFFSET_M = 0.5f;    // 측면 오프셋(attackRange × n)
    const float ALIGN_TOL = 1f;   // z축 정렬 허용 오차
    const float NO_ALIGN_TIME = 1.5f;    // WaitCool 미진입 타임아웃

    /* ─── 상태 ─── */
    float repT, chaseTimer, noAlignT;
    Vector3 sideTarget;    // 현재 측면 목표

    public ChaseState(EnemyFSM f) : base(f) { }

    // ───────────────────────────── Enter ─────────────────────────────
    public override void Enter()
    {
        repT = chaseTimer = noAlignT = 0f;

        if (agent)
        {
            SetAgentStopped(false);
            agent.speed = status.moveSpeed * status.chaseSpeedMultiplier;
            agent.stoppingDistance = status.attackRange * 0.4f;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        fsm.PlayDirectionalAnim("Chase");
    }

    // ───────────────────────────── Execute ────────────────────────────
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 0) 타깃 확인 */
        if (fsm.Target == null || !fsm.Target.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(WanderState));
            return;
        }

        /* 1) 추적 한계 */
        chaseTimer += Time.deltaTime;
        if (chaseTimer > status.maxChaseTime ||
            (fsm.Target.position - fsm.spawnPosition).sqrMagnitude >
            status.maxChaseDistance * status.maxChaseDistance)
        {
            fsm.Target = null;
            fsm.TransitionToState(typeof(ReturnState));
            return;
        }

        /* 2) ───── 측면 목표 계산 ───── */
        float side = Mathf.Sign(transform.position.x - fsm.Target.position.x);
        if (side == 0) side = 1f;                               // 정확히 중앙이면 오른쪽 선택

        sideTarget = fsm.Target.position;
        sideTarget.x += side * status.attackRange * SIDE_OFFSET_M;
        sideTarget.y = transform.position.y;              // 지면 유지

        /* 3) 경로 재계산 */
        repT += Time.deltaTime;
        if (repT >= REP_INT && !agent.pathPending)
        {
            agent.SetDestination(sideTarget);
            repT = 0f;
        }

        /* 4) 정렬 판정 */
        float zAbs = Mathf.Abs(transform.position.z - fsm.Target.position.z);
        bool inRange = fsm.GetTarget2DDistSq() <= status.attackRange * status.attackRange;

        /* 추가: 실제 속도 기반으로 “도착” 판정 */
        float velSq = agent.velocity.sqrMagnitude;        // 현재 이동 속도^2

        // > 거의 멈춘 상태 + 사정거리 + z 오차 < 허용치  → WaitCool 진입
        if (velSq < 0.01f && zAbs <= ALIGN_TOL && inRange)
        {
            agent.isStopped = true;                       // 멈춰서 Idle 애니 재생
            fsm.TransitionToState(typeof(WaitCoolState));
            return;
        }

        /* 5) WaitCool 미진입 타이머 */
        noAlignT += Time.deltaTime;

        if (noAlignT >= NO_ALIGN_TIME)
        {
            noAlignT = 0f;
            fsm.TransitionToState(typeof(DetourState));   // 한번 우회해서 재정렬
            return;
        }

        /* 6) 방향 & 애니메이션 */
        RefreshFacingToMoveOrTarget(agent.velocity);
        fsm.PlayDirectionalAnim("Chase");
    }

    // ───────────────────────────── Exit ──────────────────────────────
    public override void Exit()
    {
        if (agent) agent.autoBraking = false;
    }

    /* ────────────── 보조 ────────────── */
    void RefreshFacingToMoveOrTarget(Vector3 vel)
    {
        float dir = Mathf.Abs(vel.x) > 0.01f
            ? vel.x
            : (fsm.Target.position.x - transform.position.x);
        fsm.ForceFacing(dir);
    }
}
