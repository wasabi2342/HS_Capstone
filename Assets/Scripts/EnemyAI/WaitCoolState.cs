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
        fsm.Anim.speed = 1f;  // �ִϸ��̼� �ӵ� �ʱ�ȭ
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            waitCo = fsm.StartCoroutine(WaitRoutine());
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!fsm.IsAlignedAndInRange())
        {
            if (fsm.debugMode) Debug.Log("[WaitCool] �� ���� ���� �� Chase", fsm);
            fsm.Agent.isStopped=false;                 // Agent ��õ�
            fsm.TransitionToState(typeof(ChaseState));
            return;
        }
        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(status.waitCoolTime);

        bool canAtk = fsm.IsAlignedAndInRange();
        if (fsm.debugMode)
            Debug.Log($"[WaitCool] time={status.waitCoolTime}s, "
                    + $"dist={Mathf.Sqrt(fsm.GetTarget2DDistSq()):0.00}, "
                    + $"result={(canAtk ? "Attack" : "Chase")}", fsm);

        fsm.TransitionToState(canAtk ? typeof(AttackState) : typeof(ChaseState));
    }


    public override void Exit()
    {
        if (waitCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(waitCo);
    }
}
