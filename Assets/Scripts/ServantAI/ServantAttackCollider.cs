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
        // 1) �����ְ� �ƴ� Ŭ���̾�Ʈ�� ����
        if (!pv.IsMine) return;

        // 2) ����(Owner) ������ ����
        if (fsm.OwnerPlayer != null && other.transform.IsChildOf(fsm.OwnerPlayer)) return;

        // 3) PVE ��� Enemy ���̾ ���
        if (!fsm.IsPvpScene && other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        // 4) ���� ������ ó��
        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(fsm.attackDamage, transform.position, AttackerType.PinkPlayer);
            AudioManager.Instance.PlayOneShot("event:/Character/Character-pink/spear attack", transform.position, RpcTarget.All);
        }
    }
}
