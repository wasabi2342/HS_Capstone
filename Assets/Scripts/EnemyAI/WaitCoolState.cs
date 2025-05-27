using UnityEngine;
using System.Collections;
using Photon.Pun;

public class WaitCoolState : BaseState
{
    Coroutine waitCo;
    

    public WaitCoolState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
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

        /* z-�� Ʋ�����ų� ������ ��� �� Chase */
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
        yield return new WaitForSeconds(status.waitCoolTime);

        bool aligned = fsm.IsAlignedAndInRange();        // ���� + �����Ÿ�
        bool canAtk = aligned /* && fsm.CanAttackNow() */; // �ʿ��ϸ� ���� �߰�

        if (fsm.debugMode)
            Debug.Log($"[WaitCool] {status.waitCoolTime:F1}s �� "
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
