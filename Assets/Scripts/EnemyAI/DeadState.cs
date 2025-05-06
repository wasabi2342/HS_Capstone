using UnityEngine;
using System.Collections;
using Photon.Pun;

public class DeadState : BaseState
{
    const float DESTROY_DELAY = 1.5f;               // 원 EnemyAI DIE_DUR ≒ 1.5f
    public DeadState(EnemyFSM f) : base(f) { }

    public override void Enter()
    {
        RefreshFacingToTarget();
        SetAgentStopped(true);

        fsm.DestroyUI();                            // HP바 제거
        if (fsm.TryGetComponent(out Collider col)) col.enabled = false;

        /* 애니메이션 */
        fsm.PlayDirectionalAnim("Death");

        /*  몬스터 카운트 감소 */
        if (PhotonNetwork.IsMasterClient)
        {
            EnemyFSM.ActiveMonsterCount--;
            if (EnemyFSM.ActiveMonsterCount == 0)
                StageManager.Instance?.OnAllMonsterCleared();
        }

        /* 파괴 코루틴 */
        if (PhotonNetwork.IsMasterClient)
            fsm.StartCoroutine(DestroyLater());
    }

    public override void Execute() { }

    IEnumerator DestroyLater()
    {
        float len = 1f;
        if (animator && animator.runtimeAnimatorController)
            len = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(Mathf.Max(len, DESTROY_DELAY));
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(fsm.gameObject);
    }

    public override void Exit() { }
}
