// ========================= HitState.cs
using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.Rendering;

/// <summary>
/// �ǰ� �� ��� ����. ���� �ð��� EnemyStatusSO.hitStunTime(�⺻ 0.3f) �� �����˴ϴ�.
/// </summary>
public class HitState : BaseState
{
    Coroutine stunCo;

    public HitState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;

        fsm.PlayDirectionalAnim("Hit");

        // HP 0 �̸� ��� DeathState ��
        if (fsm.currentHP <= 0)
        {
            fsm.TransitionToState(typeof(DeadState));
            return;
        }

        if (PhotonNetwork.IsMasterClient)
            stunCo = fsm.StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        float dur = status.hitStunTime > 0 ? status.hitStunTime : .3f;
        yield return new WaitForSeconds(dur);

        if (!PhotonNetwork.IsMasterClient) yield break;

        // ���� �� �ٽ� Chase or Wander
        if (fsm.Target)
            fsm.TransitionToState(typeof(ChaseState));
        else
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Execute()
    {
        // �ִ� ���� ����
        fsm.PlayDirectionalAnim("Hit");
    }

    public override void Exit()
    {
        if (stunCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(stunCo);
    }
}
