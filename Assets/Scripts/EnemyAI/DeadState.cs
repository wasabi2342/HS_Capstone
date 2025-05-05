using UnityEngine;
using Photon.Pun;
using System.Collections;

/// <summary>
/// ��� �ִϸ��̼� ��� �� ���� �ð� �� ��Ʈ��ũ�� �ı�.
/// </summary>
public class DeadState : BaseState
{
    const float DESTROY_DELAY = 2f;

    public DeadState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        if (agent) agent.isStopped = true;
        if (fsm.TryGetComponent(out Collider col)) col.enabled = false;

        fsm.PlayDirectionalAnim("Death");

        if (PhotonNetwork.IsMasterClient)
            fsm.StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        float clipLen = 1f;

        if (fsm.Anim && fsm.Anim.runtimeAnimatorController)          
        {
            var state = fsm.Anim.GetCurrentAnimatorStateInfo(0);     
            clipLen = state.length;
        }

        yield return new WaitForSeconds(clipLen + DESTROY_DELAY);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(fsm.gameObject);                   
    }

    public override void Execute() { }
    public override void Exit() { }
}
