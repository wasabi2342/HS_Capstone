using UnityEngine;

public class ScarecrowHit : IState
{
    readonly ScarecrowFSM fsm;
    float stun;
    public ScarecrowHit(ScarecrowFSM f) { fsm = f; }

    public void Enter()
    {
        Debug.Log("[ScarecrowHit] Enter Hit State ▶ 애니메이션 재생");
        fsm.PlayAnim("Hit");
        stun = fsm.hitStunTime;
    }

    public void Execute()
    {
        stun -= Time.deltaTime;
        if (stun <= 0f)
            fsm.TransitionTo(typeof(ScarecrowIdle));
    }
    public void Exit() { }
}
