using UnityEngine;

public class ScarecrowIdle : IState
{
    readonly ScarecrowFSM fsm;
    public ScarecrowIdle(ScarecrowFSM f) { fsm = f; }

    public void Enter()
    {
        fsm.PlayAnim("Idle");
        // Idle�� ���� �� Ÿ�̸� �ʱ�ȭ X (Hit �� Idle �ÿ� �̹� 0���� ����)
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
