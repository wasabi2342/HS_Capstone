using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class ServantAttackCollider : MonoBehaviourPun
{
    ServantFSM fsm;
    PhotonView pv;

    void Awake()
    {
        fsm = GetComponentInParent<ServantFSM>();
        pv = fsm.GetComponent<PhotonView>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 1) 소유주가 아닌 클라이언트는 무시
        if (!pv.IsMine) return;

        // 2) 주인(Owner) 맞으면 무시
        if (fsm.OwnerPlayer != null && other.transform.IsChildOf(fsm.OwnerPlayer)) return;

        // 3) PVE 라면 Enemy 레이어만 통과
        if (!fsm.IsPvpScene && other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        // 4) 실제 데미지 처리
        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(fsm.attackDamage, transform.position, AttackerType.PinkPlayer);
            AudioManager.Instance.PlayOneShot("event:/Character/Character-pink/spear attack", transform.position, RpcTarget.All);
        }
    }
}
