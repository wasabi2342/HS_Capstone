// ========================= HitState.cs
using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.Rendering;

/// <summary>
/// 피격 후 잠시 경직. 경직 시간은 EnemyStatusSO.hitStunTime(기본 0.3f) 로 결정됩니다.
/// </summary>
public class HitState : BaseState
{
    Coroutine stunCo;

    public HitState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;

        fsm.PlayDirectionalAnim("Hit");

        // HP 0 이면 즉시 DeathState 로
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

        // 스턴 후 다시 Chase or Wander
        if (fsm.Target)
            fsm.TransitionToState(typeof(ChaseState));
        else
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Execute()
    {
        // 애니 방향 유지
        fsm.PlayDirectionalAnim("Hit");
    }

    public override void Exit()
    {
        if (stunCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(stunCo);
    }
}
