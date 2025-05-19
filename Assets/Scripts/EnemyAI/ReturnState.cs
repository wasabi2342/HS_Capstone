using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
public class ReturnState : BaseState
{
    public ReturnState(EnemyFSM f) : base(f) { }
    void RefreshFacingToSpawn()
    {
        float dx = fsm.spawnPosition.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)          // 1cm ��� ����
            fsm.ForceFacing(dx);
    }

    public override void Enter()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;
        agent.SetDestination(fsm.spawnPosition);

        RefreshFacingToSpawn();             // ���� ����
        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Execute()
    {
        RefreshFacingToSpawn();             // ��-������ ����ȭ
        fsm.PlayDirectionalAnim("Walk");

        if (fsm.Agent.remainingDistance <= fsm.Agent.stoppingDistance + .05f)
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Exit() => fsm.Agent.isStopped = true;
}

