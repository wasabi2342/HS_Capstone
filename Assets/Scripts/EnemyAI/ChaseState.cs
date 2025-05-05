// ========================= ChaseState.cs
using UnityEngine;
using Photon.Pun;

public class ChaseState : BaseState
{
    private float atkChkT, destT;
    public ChaseState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent)
        {
            agent.isStopped = false;
            agent.speed = status.moveSpeed * (status.chaseSpeedMultiplier > 0 ? status.chaseSpeedMultiplier : 1f);
        }
        fsm.PlayDirectionalAnim("Walk");
        atkChkT = destT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient || !agent) return;
        if (!fsm.Target || !fsm.Target.gameObject.activeInHierarchy)
        {
            fsm.TransitionToState(typeof(WanderState)); return;
        }

        destT += Time.deltaTime;
        if (destT >= .3f) { agent.SetDestination(fsm.Target.position); destT = 0f; }

        float distSq = (fsm.Target.position - transform.position).sqrMagnitude;

        if (distSq > status.detectRange * status.detectRange)
        {
            fsm.Target = null;
            fsm.TransitionToState(typeof(WanderState)); return;
        }

        atkChkT += Time.deltaTime;
        if (atkChkT >= .1f)
        {
            atkChkT = 0f;
            if (distSq <= status.attackRange * status.attackRange)
            {
                fsm.TransitionToState(typeof(WaitCoolState)); return;
            }
        }
        fsm.PlayDirectionalAnim("Walk");
    }

    public override void Exit() { }
}
