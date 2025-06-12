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

        // 3) PVE ��� Enemy ���̾ Ÿ��
        if (!fsm.IsPvpScene && other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        // 4) ���� ������ ó��
        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            // ����: ��ȯ���� ���� ��ġ(fsm.transform.position) ����
            // AttackerType�� Servant�� ����
            dmg.TakeDamage(fsm.attackDamage, fsm.transform.position, AttackerType.Servant);
            AudioManager.Instance.PlayOneShot("event:/Character/Character-pink/spear attack", transform.position, RpcTarget.All);
        }
    }
}