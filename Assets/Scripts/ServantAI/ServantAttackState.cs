// ServantAttackState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantAttackState : ServantBaseState
{
    private Coroutine atkCo;

    public ServantAttackState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        float facing = fsm.CurrentFacing;              // +1 �̸� ��, -1 �̸� ��
        fsm.Attack.SetDirection(facing);

        // �ִϸ��̼� ���
        PlayDirectionalAnim("Attack");

        // �ݶ��̴� ó�� ����
        if (PhotonNetwork.IsMasterClient)
            atkCo = fsm.StartCoroutine(AttackRoutine());
    }

    public override void Execute()
    {
        // ���������� ���� ���� (optional)
        if (!PhotonNetwork.IsMasterClient) return;
        RefreshFacingToTarget();
        PlayDirectionalAnim("Attack");
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(fsm.attackDuration * 0.3f);

        fsm.Attack.SetDirection(fsm.CurrentFacing);
        fsm.Attack.EnableAttack();

        yield return new WaitForSeconds(fsm.attackDuration * 0.4f);

        fsm.Attack.DisableAttack();

        yield return new WaitForSeconds(fsm.attackDuration * 0.3f);
        fsm.TransitionToState(typeof(ServantAttackCoolState));
    }

    public override void Exit()
    {
        // Ȥ�� ���������� ���ֱ�
        if (atkCo != null)
            fsm.StopCoroutine(atkCo);
        fsm.Attack.DisableAttack();
    }
}
