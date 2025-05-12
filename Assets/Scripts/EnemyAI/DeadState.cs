using UnityEngine;
using System.Collections;
using Photon.Pun;

public class DeadState : BaseState
{
    const float DESTROY_DELAY = 1.5f;               // 원 EnemyAI DIE_DUR ≒ 1.5f
    private float destroyTimer = 0f;
    
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
            
        destroyTimer = 0f;  // 타이머 초기화
    }

    public override void Execute() 
    {
        // 타이머 업데이트
        destroyTimer += Time.deltaTime;
    }

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
    
    // EnemyFSM에서 파괴 시기를 확인하는 메서드 (필요 없음 - 코루틴에서 처리)
    public bool ShouldDestroy()
    {
        return false; // 항상 false 반환 - 코루틴에서 처리하므로
    }
}
