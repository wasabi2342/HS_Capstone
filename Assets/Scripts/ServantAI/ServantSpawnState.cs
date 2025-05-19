using UnityEngine;
using System.Collections;
using Photon.Pun;

public class ServantSpawnState : ServantBaseState
{
    private Coroutine invCo;
    private float invincibleTime = 1f;

    public ServantSpawnState(ServantFSM fsm) : base(fsm) { }

    public override void Enter()
    {
        // 1) ���� �ִϸ��̼�
        fsm.Anim.Play("Spawn", 0);
        if (fsm.pv.IsMine)
            fsm.pv.RPC(nameof(fsm.RPC_PlayClip), RpcTarget.Others, "Spawn");

        // 2) ��� ����
        fsm.IsInvincible = true;

        // 4) ���� ���� Ÿ�̸�
        invCo = fsm.StartCoroutine(InvincibleRoutine());
    }

    public override void Execute()
    {
        // (���Ͻø� ����ٸ� ���·� �ڵ� ��ȯ�� ���� �߰�)
    }

    public override void Exit()
    {
        if (invCo != null)
            fsm.StopCoroutine(invCo);
        fsm.IsInvincible = false;
    }

    private IEnumerator InvincibleRoutine()
    {
        yield return new WaitForSeconds(invincibleTime);
        fsm.IsInvincible = false;
        fsm.TransitionToState(typeof(ServantIdleState));
    }
}
