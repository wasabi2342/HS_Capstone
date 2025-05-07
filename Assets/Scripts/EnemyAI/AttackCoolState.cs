using UnityEngine;
using System.Collections;
using Photon.Pun;

public class AttackCoolState : BaseState
{
    Coroutine coolCo;
    public AttackCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            coolCo = fsm.StartCoroutine(CoolTime());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator CoolTime()
    {
        // ���� �� ��Ÿ�� ���
        yield return new WaitForSeconds(status.attackCoolTime);
        if (!PhotonNetwork.IsMasterClient) yield break;

        // �÷��̾ ���� ����(�Ÿ� & Z) ������ �����ϸ� Wander�� ���� �ʰ� �ٷ� WaitCoolState�� ��ȯ
        bool immediate = fsm.Target && fsm.IsAlignedAndInRange();

        fsm.TransitionToState(immediate ? typeof(WaitCoolState) : typeof(WanderState));
    }

    public override void Exit()
    {
        if (coolCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(coolCo);
    }
}
