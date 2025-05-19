using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class SpawnAttackCollider : MonoBehaviourPun
{
    ServantFSM fsm;
    PhotonView pv;
    Collider col;
    [SerializeField] float spawnDamage = 20f; // ���ݷ�

    void Awake()
    {
        // �θ� �ִ� ServantFSM�� PhotonView ĳ��
        fsm = GetComponentInParent<ServantFSM>();
        if (fsm == null)
            Debug.LogError("ServantAttackCollider: �θ� ServantFSM�� �����ϴ�!");
        col = GetComponent<Collider>();
        col.isTrigger = true; // Trigger�� ����

        pv = fsm.GetComponent<PhotonView>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 1) �����ָ� ó��
        if (!pv.IsMine) return;

        // 2) �±׷� Enemy�� ����
        if (!other.CompareTag("Enemy")) return;

        // 3) IDamageable ã�Ƽ� ������ RPC
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            // ServantFSM �� RPC_TakeDamage ȣ��
            pv.RPC(nameof(ServantFSM.RPC_TakeDamage), RpcTarget.AllBuffered,
                   spawnDamage, pv.ViewID);
        }
    }
}
