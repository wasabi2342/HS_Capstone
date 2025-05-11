using UnityEngine;

public class ScarecrowIdle : IState
{
    readonly ScarecrowFSM fsm;
    public ScarecrowIdle(ScarecrowFSM f) { fsm = f; }

    public void Enter()
    {
        fsm.PlayAnim("Idle");
        // Idle에 들어올 때 타이머 초기화 X (Hit → Idle 시엔 이미 0으로 리셋)
    }

    public void Execute()
    {
        if (!fsm.AttackEnabled) return;

        fsm.attackTimer += Time.deltaTime;
        if (fsm.attackTimer >= fsm.attackInterval)
        {
            fsm.attackTimer = 0f;
            fsm.TransitionTo(typeof(ScarecrowAttack));
        }
    }

    public void Exit() { }
}
