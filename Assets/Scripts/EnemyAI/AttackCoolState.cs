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
        bool aligned = fsm.IsAlignedAndInRange();
        /* 2) ��Ÿ���� ���� �� �б� */
        if (aligned)       // Ž�� �ݰ� �� �� ���� �簳
        {
            SetAgentStopped(true);
            fsm.TransitionToState(typeof(WaitCoolState));
        }
        else                                   // �־��� �� ����/��ȯ
        {
            SetAgentStopped(false);
            fsm.TransitionToState(typeof(WanderState));  // �ʿ��ϸ� ReturnState
        }
    }

    /* ���� ���� �� ������������������������������������������������������������ */
    public override void Exit()
    {
        SetAgentStopped(false);                // ���� ���°� �̵� �����ϵ���
    }
}
