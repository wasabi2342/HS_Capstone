// ServantHitState.cs
using UnityEngine;

public class ServantHitState : ServantBaseState
{
    float hitDuration = 0.3f;
    float timer;

    public ServantHitState(ServantFSM servantFSM) : base(servantFSM) { }

    public override void Enter()
    {
        agent.isStopped = true;
        // 현재 Facing을 유지하기 위해 위치 기반으로 갱신
        RefreshFacingToTarget();
        PlayDirectionalAnim("Hit");
        timer = 0f;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= hitDuration)
            fsm.TransitionToState(typeof(ServantWanderState));
    }

    public override void Exit() { }
}
