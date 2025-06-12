using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantIdleState : ServantBaseState
{
    Coroutine idleCo;
    float detectT;

    public ServantIdleState(ServantFSM s) : base(s) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);
        PlayDirectionalAnim("Idle");
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
            fsm.DetectEnemy();
            detectT = 0f;
            if (fsm.TargetEnemy != null)
            {
                fsm.TransitionToState(typeof(ServantChaseState));
                return;
            }
        }
        RefreshFacingToTarget();
        PlayDirectionalAnim("Idle");
    }

    IEnumerator IdleTimer()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        if (PhotonNetwork.IsMasterClient && fsm.CurrentState == this)
            fsm.TransitionToState(typeof(ServantWanderState));
    }

    public override void Exit()
    {
        if (idleCo != null && PhotonNetwork.IsMasterClient)
            fsm.StopCoroutine(idleCo);
    }
}
