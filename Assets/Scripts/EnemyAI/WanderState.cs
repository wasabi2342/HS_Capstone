using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class WanderState : BaseState
{
    const float IDLE_CHANCE = 0.5f;
    const float DETECT_INTERVAL = 0.2f;

    float detectT, repathT;

    public WanderState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent)
        {
            SetAgentStopped(false);
            agent.speed = status.moveSpeed;
        }
        fsm.PlayDirectionalAnim("Walk");

        PickDestination();
        detectT = repathT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 플레이어 탐지
        detectT += Time.deltaTime;
        if (detectT >= DETECT_INTERVAL)
        {
            fsm.DetectPlayer();
            detectT = 0f;
            if (fsm.Target)
            {
                fsm.TransitionToState(typeof(ChaseState));
                return;
            }
        }

        // 목적지 도달 또는 막힘 처리
        repathT += Time.deltaTime;
        if ((!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) ||
            (repathT >= 3f && agent.velocity.sqrMagnitude < 0.01f))
        {
            if (Random.value < IDLE_CHANCE)
                fsm.TransitionToState(typeof(IdleState));
            else
                PickDestination();

            repathT = 0f;
        }

        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }

    void PickDestination()
    {
        Vector3 raw;

        if (fsm.CurrentSpawnArea != null)                                 // SpawnArea 우선
        {
            raw = fsm.CurrentSpawnArea.GetRandomPointInsideArea();
        }
        else if (status.spawnAreaBounds.size != Vector3.zero)             // SO Bounds
        {
            Bounds b = status.spawnAreaBounds;
            raw = new Vector3(
                Random.Range(b.min.x, b.max.x),
                transform.position.y,
                Random.Range(b.min.z, b.max.z));
        }
        else                                                             // 기본 랜덤 반경 4m 유지
        {
            raw = transform.position + Random.insideUnitSphere * 4f;
            raw.y = transform.position.y;
        }

        // SO.navMeshSampleDistance 사용
        if (NavMesh.SamplePosition(
                raw,
                out NavMeshHit hit,
                status.navMeshSampleDistance,
                NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
