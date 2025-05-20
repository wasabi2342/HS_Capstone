using UnityEngine;
using System.Collections;
using Photon.Pun;

public class IdleState : BaseState
{
    Coroutine idleCo;
    float detectT;

    public IdleState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            idleCo = fsm.StartCoroutine(IdleTimer());
        fsm.Anim.speed = 1f;  // 애니메이션 속도 초기화
        detectT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        detectT += Time.deltaTime;
        if (detectT >= 0.2f)
        {
            fsm.DetectTarget();
            detectT = 0f;
            if (fsm.Target) { fsm.TransitionToState(typeof(ChaseState)); return; }
        }

        RefreshFacingToTarget();
        fsm.PlayDirectionalAnim("Idle");
    }

    IEnumerator IdleTimer()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        if (PhotonNetwork.IsMasterClient && fsm.CurrentState == this)
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Exit()
    {
        if (idleCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(idleCo);
    }
}
