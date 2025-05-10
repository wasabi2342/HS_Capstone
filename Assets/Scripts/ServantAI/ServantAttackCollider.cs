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
            Debug.LogError("[ServantAttackCollider] �θ� ServantFSM�� �����ϴ�!");

        col = GetComponent<Collider>();
        col.isTrigger = true;
        gameObject.SetActive(false);  // EnableAttack() �� Ȱ��ȭ
    }

    void OnTriggerEnter(Collider other)
    {
        // MasterClient ����, �� ��ȯ���� ó��
        if (!PhotonNetwork.IsMasterClient) return;
        if (!photonView.IsMine) return;

        // Enemy ���̾� �˻�
        int layer = other.gameObject.layer;
        if (((1 << layer) & fsm.enemyLayerMask.value) == 0)
            return;

        // IDamageable �������̽��� ������ ����
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(fsm.attackDamage, fsm.transform.position, AttackerType.PinkPlayer);
    }
}
