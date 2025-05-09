// ServantWanderState.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ServantWanderState : ServantBaseState
{
    const float IDLE_CHANCE = 0.5f;  // Idle 전환 확률
    const float DETECT_INTERVAL = 0.2f;  // 적 탐지 주기
    const float MIN_WANDER_TIME = 0.2f;  // 최소 이동 시간

    float detectT, repathT;

    public ServantWanderState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        SetAgentStopped(false);
        agent.speed = fsm.moveSpeed;
        Vector3 vel = agent.velocity;
        if (vel.x > 0) fsm.ForceFacing(1f);
        else if (vel.x < 0) fsm.ForceFacing(-1f);
        PlayDirectionalAnim("Walk");
        PickDestinationAroundOwner();
        detectT = repathT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Vector3 vel = agent.velocity;
        if (vel.x > 0.01f) fsm.ForceFacing(+1f);
        else if (vel.x < -0.01f) fsm.ForceFacing(-1f);
        // 1) 적 탐지
        detectT += Time.deltaTime;
        if (detectT >= DETECT_INTERVAL)
        {
            fsm.DetectEnemy();
            detectT = 0f;
            if (fsm.TargetEnemy != null)
            {
                fsm.TransitionToState(typeof(ServantChaseState));
                return;
            }
        }

        // 2) 목적지 도달/막힘 판정
        repathT += Time.deltaTime;
        bool arrived = !agent.pathPending
                       && agent.remainingDistance <= agent.stoppingDistance
                       && repathT >= MIN_WANDER_TIME;      // 최소 시간 경과
        bool blocked = repathT >= 3f
                       && agent.velocity.sqrMagnitude < 0.01f;

        if (arrived || blocked)
        {
            if (Random.value < IDLE_CHANCE)
                fsm.TransitionToState(typeof(ServantIdleState));
            else
                PickDestinationAroundOwner();
            repathT = 0f;
        }

        PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }

    void PickDestinationAroundOwner()
    {
        if (fsm.OwnerPlayer == null) return;
        Vector3 basePos = fsm.OwnerPlayer.position;
        Vector3 raw = basePos + Random.insideUnitSphere * 4f;
        raw.y = basePos.y;
        if (NavMesh.SamplePosition(raw, out var hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
