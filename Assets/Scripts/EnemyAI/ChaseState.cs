using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

/// <summary>
/// attackRange-비례 + “플레이어 폭”을 고려해 측면 오프셋을 자동 보정한 ChaseState.
///    들어갈 때(IN) · 나올 때(OUT) 히스테리시스 적용으로 Idle↔Chase 깜빡임 방지  
///    일정 시간 z-정렬 실패 시 DetourState로 1회 우회  
/// </summary>
public class ChaseState : BaseState
{
    /* ─── 비율 상수 ─── */
    const float STOP_RATIO = 0.10f;  // 멈춤 거리 = attackRange × 0.10
    const float SIDE_RATIO = 0.15f;  // 기본 측면 오프셋 = attackRange × 0.15
    const float IN_RATIO = 0.30f;  // WaitCool 진입 z-오차
    const float OUT_RATIO = 0.37f;  // WaitCool 이탈 z-오차

    const float MIN_STOP = 0.08f;     // 단검 몬스터 최소 멈춤 8 cm
    const float MIN_SIDE = 0.10f;     // 최소 측면 10 cm

    const float REP_INT = 0.18f;
    const float NO_ALIGN_TIME = 1.5f;

    /* ─── 동적으로 계산되는 값 ─── */
    float stopDist, sideOff, tolIn, tolOut;

    /* ─── 상태 변수 ─── */
    float repT, chaseT, noAlignT;
    Vector3 sideTarget;
    float lastSide = 1f;              // 0 나올 때 이전 값 유지

    internal float TolOut => tolOut;  // WaitCoolState에서 읽음

    public ChaseState(EnemyFSM f) : base(f) { }

    /*────────────────────────── Enter ─────────────────────────*/
    public override void Enter()
    {
        float atkR = status.attackRange;

        /* 1) 플레이어 Collider 반경 추출 */
        float playerRad = 0.25f;          // 기본값
        if (fsm.Target && fsm.Target.TryGetComponent(out CapsuleCollider pc))
            playerRad = pc.radius;

        /* 2) 비례 × 클램프 계산 */
        stopDist = Mathf.Max(MIN_STOP, atkR * STOP_RATIO);
        sideOff = Mathf.Max(MIN_SIDE,
                             atkR * SIDE_RATIO,
                             playerRad + stopDist + 0.05f);  // ▶ 플레이어 폭+여유
        tolIn = atkR * IN_RATIO;
        tolOut = atkR * OUT_RATIO;

        repT = chaseT = noAlignT = 0f;

        if (agent)
        {
            SetAgentStopped(false);
            agent.speed = status.moveSpeed * status.chaseSpeedMultiplier;
            agent.stoppingDistance = stopDist;
            agent.autoBraking = false;
            agent.acceleration = 40;
            agent.angularSpeed = 720;
            agent.SetDestination(transform.position); // 목적지 초기화
        }

        fsm.PlayDirectionalAnim("Chase");
        fsm.TolOutCache = tolOut;                      // WaitCoolState용
    }

    /*───────────────────────── Execute ────────────────────────*/
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        fsm.DetectTarget();
        /* 0) 타깃 유효/추적 한계 */
        if (fsm.Target == null || !fsm.Target.gameObject.activeInHierarchy)
        { fsm.TransitionToState(typeof(WanderState)); return; }

        chaseT += Time.deltaTime;
        if (chaseT > status.maxChaseTime ||
            (fsm.Target.position - fsm.spawnPosition).sqrMagnitude >
            status.maxChaseDistance * status.maxChaseDistance)
        { fsm.Target = null; fsm.TransitionToState(typeof(ReturnState)); return; }

        /* 1) 측면 슬롯 목표 */
        float sideRaw = Mathf.Sign(transform.position.x - fsm.Target.position.x);
        float side = sideRaw != 0 ? sideRaw : lastSide;
        lastSide = side;

        sideTarget = fsm.Target.position;
        sideTarget.x += side * sideOff;
        sideTarget.y = transform.position.y;

        /* 2) 경로 갱신 */
        repT += Time.deltaTime;
        if (repT >= REP_INT && !agent.pathPending)
        {
            if ((sideTarget - agent.destination).sqrMagnitude > 0.0025f)
                agent.SetDestination(sideTarget);
            repT = 0f;
        }

        /* 3) 판정 */
        float xAbs = Mathf.Abs(transform.position.x - sideTarget.x);
        float zAbs = Mathf.Abs(transform.position.z - fsm.Target.position.z);
        bool inAtk = fsm.IsTargetInAttackRange();
        bool reached = xAbs <= stopDist + 0.05f;
        float velSq = agent.velocity.sqrMagnitude;

        /* ▶ WaitCool 진입 */
        if (velSq < 0.01f && reached && zAbs <= tolIn && inAtk)
        {
            agent.isStopped = true;
            fsm.TransitionToState(typeof(WaitCoolState)); return;
        }

        /* 정렬 실패 타이머 */
        bool needAlign = reached && zAbs > tolOut && inAtk;
        noAlignT = needAlign ? noAlignT + Time.deltaTime : 0f;

        if (noAlignT >= NO_ALIGN_TIME)
        { noAlignT = 0f; fsm.TransitionToState(typeof(DetourState)); return; }

        /* 4) 애니메이션 */
        RefreshFacingToMoveOrTarget(agent.velocity);
        fsm.PlayDirectionalAnim("Chase");
    }

    /*────────────────────────── Exit ─────────────────────────*/
    public override void Exit()
    {
        if (agent) agent.autoBraking = true;
    }

    /*──────── Direction helper ────────*/
    void RefreshFacingToMoveOrTarget(Vector3 vel)
    {
        float dir = Mathf.Abs(vel.x) > 0.01f ? vel.x
                                             : (fsm.Target.position.x - transform.position.x);
        fsm.ForceFacing(dir);
    }
}
