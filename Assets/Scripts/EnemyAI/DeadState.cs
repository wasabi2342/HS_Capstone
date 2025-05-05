using UnityEngine;
using System.Collections;
using Photon.Pun;

public class DeadState : BaseState
{
    const float DESTROY_DELAY = 2f;
    public DeadState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);

        if (fsm.TryGetComponent(out Collider col)) col.enabled = false;

        fsm.PlayDirectionalAnim("Death");

        if (PhotonNetwork.IsMasterClient)
            fsm.StartCoroutine(DestroyLater());
    }

    public override void Execute() { }

    IEnumerator DestroyLater()
    {
        float len = 1f;
        if (animator && animator.runtimeAnimatorController)
            len = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(len + DESTROY_DELAY);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(fsm.gameObject);
    }

    public override void Exit() { }
}
