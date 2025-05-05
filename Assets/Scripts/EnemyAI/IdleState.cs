// ========================= IdleState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class IdleState : BaseState
{
    private Coroutine idleCo;
    private float detectT;

    public IdleState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;
        fsm.PlayDirectionalAnim("Idle");

        if (PhotonNetwork.IsMasterClient)
            idleCo = fsm.StartCoroutine(IdleTimer());

        detectT = 0f;
    }

    public override void Execute()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        detectT += Time.deltaTime;
        if (detectT >= 0.2f)
        {
            fsm.DetectPlayer();
            detectT = 0f;
            if (fsm.Target != null)
            {
                fsm.TransitionToState(typeof(ChaseState));
                return;
            }
        }
        fsm.PlayDirectionalAnim("Idle");
    }

    private IEnumerator IdleTimer()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        if (PhotonNetwork.IsMasterClient)
            fsm.TransitionToState(typeof(WanderState));
    }

    public override void Exit()
    {
        if (idleCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(idleCo);
    }
}
