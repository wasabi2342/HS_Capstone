using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;
public class ReturnState : BaseState
{
    public ReturnState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        fsm.Agent.isStopped = false;
        fsm.Agent.SetDestination(fsm.spawnPosition);
        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Execute()
    {
        if (fsm.Agent.remainingDistance <= fsm.Agent.stoppingDistance + .05f)
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Exit() => fsm.Agent.isStopped = true;
}

