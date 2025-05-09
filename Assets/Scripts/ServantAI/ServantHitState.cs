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
        // ���� Facing�� �����ϱ� ���� ��ġ ������� ����
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
