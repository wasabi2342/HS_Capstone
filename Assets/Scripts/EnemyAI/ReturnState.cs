using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
public class ReturnState : BaseState
{
    public ReturnState(EnemyFSM f) : base(f) { }
    void RefreshFacingToSpawn()
    {
        float dx = fsm.spawnPosition.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)          // 1cm 허용 오차
            fsm.ForceFacing(dx);
    }

    public override void Enter()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.SetDestination(fsm.spawnPosition);

        RefreshFacingToSpawn();             // 방향 먼저
        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Execute()
    {
        RefreshFacingToSpawn();             // 매-프레임 동기화
        fsm.PlayDirectionalAnim("Walk");

        if (fsm.Agent.remainingDistance <= fsm.Agent.stoppingDistance + .05f)
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Exit() => fsm.Agent.isStopped = true;
}

