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
        // 1) 스폰 애니메이션
        fsm.Anim.Play("Spawn", 0);
        if (fsm.pv.IsMine)
            fsm.pv.RPC(nameof(fsm.RPC_PlayClip), RpcTarget.Others, "Spawn");

        // 2) 잠시 무적
        fsm.IsInvincible = true;

        // 4) 무적 해제 타이머
        invCo = fsm.StartCoroutine(InvincibleRoutine());
    }

    public override void Execute()
    {
        // (원하시면 대기→다른 상태로 자동 전환할 로직 추가)
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
