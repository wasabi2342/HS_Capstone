using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class ServantAttackCollider : MonoBehaviourPun
{
    private ServantFSM fsm;
    private Collider col;

    void Awake()
    {
        fsm = GetComponentInParent<ServantFSM>();
        if (fsm == null)
            Debug.LogError("[ServantAttackCollider] 부모에 ServantFSM이 없습니다!");

        col = GetComponent<Collider>();
        col.isTrigger = true;
        gameObject.SetActive(false);  // EnableAttack() 시 활성화
    }

    void OnTriggerEnter(Collider other)
    {
        // MasterClient 권한, 내 소환수만 처리
        if (!PhotonNetwork.IsMasterClient) return;
        if (!photonView.IsMine) return;

        // Enemy 레이어 검사
        int layer = other.gameObject.layer;
        if (((1 << layer) & fsm.enemyLayerMask.value) == 0)
            return;

        // IDamageable 인터페이스로 데미지 전달
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(fsm.attackDamage, fsm.transform.position, AttackerType.PinkPlayer);
    }
}
