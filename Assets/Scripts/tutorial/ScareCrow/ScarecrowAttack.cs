using System.Collections;
using UnityEngine;

public class ScarecrowAttack : IState
{
    readonly ScarecrowFSM fsm;
    Coroutine co;
    public ScarecrowAttack(ScarecrowFSM f) { fsm = f; }

    public void Enter()
    {
        fsm.PlayAnim("Attack");
        co = fsm.StartCoroutine(Routine());
    }
    public void Execute() { }
    public void Exit() 
    {
        if (co != null) fsm.StopCoroutine(co);
    }
    /* ���� Attack �ִ� ������ Idle�� ���� ���� */
    IEnumerator Routine()
    {
        // �ִϸ��̼� ����(attackDur) ��ŭ ���
        yield return new WaitForSeconds(fsm.attackDur);

        /* ���� �ֱ⸦ ���� Idle�� ��ȯ.
           attackEnabled ���� �״�� �����ǹǷ�
           Idle �� attackInterval(3��) ��� �� Attack �� �ݺ� */
        fsm.TransitionTo(typeof(ScarecrowIdle));

        // Idle�� �Ѿ�� Idle.Execute()���� attackTimer�� �ٽ� ���� ����
    }
}
