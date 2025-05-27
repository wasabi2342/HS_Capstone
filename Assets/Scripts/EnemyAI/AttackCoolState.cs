using UnityEngine;
using Photon.Pun;

public class AttackCoolState : BaseState
{
    float timer;

    public AttackCoolState(EnemyFSM f) : base(f) { }

    /* ���� ���� �� ������������������������������������������������������������ */
    public override void Enter()
    {
        timer = 0f;
        SetAgentStopped(true);                 // ���ڸ� ���
        fsm.PlayDirectionalAnim("Idle");
    }

    /* ���� �� ������ ������������������������������������������������������������ */
    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* 1) ��Ÿ�� ī��Ʈ */
        timer += Time.deltaTime;
        if (timer < status.attackCoolTime)     // �� ��Ÿ�� ���� ������ �ƹ��͵� �� ��
        {
            RefreshFacingToTarget();           // �ٶ󺸴� ���⸸ ����
            return;
        }

        /* 2) ��Ÿ���� ���� �� �б� */
        if (fsm.IsTargetInDetectRange())       // Ž�� �ݰ� �� �� ���� �簳
        {
            SetAgentStopped(false);
            fsm.TransitionToState(typeof(ChaseState));
        }
        else                                   // �־��� �� ����/��ȯ
        {
            fsm.Target = null;
            fsm.TransitionToState(typeof(WanderState));  // �ʿ��ϸ� ReturnState
        }
    }

    /* ���� ���� �� ������������������������������������������������������������ */
    public override void Exit()
    {
        SetAgentStopped(false);                // ���� ���°� �̵� �����ϵ���
    }
}
