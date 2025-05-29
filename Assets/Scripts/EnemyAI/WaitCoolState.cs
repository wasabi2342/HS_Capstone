using UnityEngine;
using System.Collections;
using Photon.Pun;

public class WaitCoolState : BaseState
{
    Coroutine waitCo;
    float waitTime;                     // �̹� ����Ŭ�� ���� ��� �ð�

    public WaitCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        // �̹��� ����� ��� �ð��� 0.01~0.10 �� ���̿��� ����
        waitTime = Random.Range(0.01f, 0.10f);

        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.Anim.speed = 1f;
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
        {
            if (waitCo != null) fsm.StopCoroutine(waitCo);   // �ߺ� ����
            waitCo = fsm.StartCoroutine(WaitRoutine());
        }
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* z-���� Ʋ�����ų� ������ ��� �� Chase ��ȯ */
        float zAbs = Mathf.Abs(transform.position.z - fsm.Target.position.z);
        if (zAbs > fsm.TolOutCache || !fsm.IsTargetInAttackRange())
        {
            if (fsm.debugMode) Debug.Log("[WaitCool] �� ���� ���� �� Chase", fsm);
            SetAgentStopped(false);
            fsm.TransitionToState(typeof(ChaseState));
            return;
        }

        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator WaitRoutine()
    {
        /* ���� ��� */
        yield return new WaitForSeconds(waitTime);

        bool aligned = fsm.IsAlignedAndInRange();        // ���� + �����Ÿ�
        bool canAtk = aligned; 

        if (fsm.debugMode)
            Debug.Log($"[WaitCool] {waitTime:F2}s �� "
                    + (canAtk ? "Attack" : aligned ? "Detour" : "Chase"), fsm);

        if (canAtk)
            fsm.TransitionToState(typeof(AttackState));
        else if (aligned)          // ���� OK���� ���� �Ұ�(���� ��) ��
            fsm.TransitionToState(typeof(DetourState));
        else
            fsm.TransitionToState(typeof(ChaseState));
    }

    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
        {
            fsm.StopCoroutine(waitCo);
            waitCo = null;
        }
    }
}
