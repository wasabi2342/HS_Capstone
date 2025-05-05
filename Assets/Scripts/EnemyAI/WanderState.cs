// ========================= WanderState.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class WanderState : BaseState
{
    private const float IDLE_CHANCE = 0.5f;
    private const float DETECT_INTERVAL = 0.2f; 
    private const float MAX_SAMPLE_DISTANCE = 3f; // NavMesh.SamplePosition()에서 사용할 최대 거리
    private float detectT, repathT;


    public WanderState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent)
        {
            agent.isStopped = false;
            agent.speed = status.moveSpeed;
        }
        fsm.PlayDirectionalAnim("Walk");

        PickNewDestination();
        detectT = repathT = Time.time + 3f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 1) 플레이어 탐지 */
        detectT += Time.deltaTime;
        if (detectT >= DETECT_INTERVAL)
        {
            fsm.DetectPlayer();
            detectT = 0f;
            if (fsm.Target) { fsm.TransitionToState(typeof(ChaseState)); return; }
        }

        /* 2) 목적지 도착 or 멈춤 → 새 목적지 */
        if ((!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) ||
            (Time.time >= repathT && agent.velocity.sqrMagnitude < 0.01f))
        {
            if (Random.value < IDLE_CHANCE)
                fsm.TransitionToState(typeof(IdleState));
            else
                PickNewDestination();

            repathT = Time.time + 3f;
        }
        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }

    private void PickNewDestination()
    {
        Vector3 rawTarget;
        if (fsm.SpawnAreaRef)
            rawTarget = fsm.SpawnAreaRef.GetRandomPointInsideArea();
        else
        {
            Bounds b = status.spawnAreaBounds;
            rawTarget = new Vector3(Random.Range(b.min.x, b.max.x),
                                    transform.position.y,
                                    Random.Range(b.min.z, b.max.z));
        }

        if (NavMesh.SamplePosition(rawTarget, out var hit, MAX_SAMPLE_DISTANCE, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
}
