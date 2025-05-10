// ServantDeadState.cs
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantDeadState : ServantBaseState
{
    public ServantDeadState(ServantFSM s) : base(s) { }

    public override void Enter()
    {
        SetAgentStopped(true);
        // Facing 기반으로 Death 애니메이션 재생
        RefreshFacingToTarget();
        PlayDirectionalAnim("Death");

        if (PhotonNetwork.IsMasterClient && fsm != null)
            fsm.StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (PhotonNetwork.IsMasterClient && fsm != null && fsm.gameObject != null)
            PhotonNetwork.Destroy(fsm.gameObject);
    }

    public override void Execute() { }
    public override void Exit() { }
}
