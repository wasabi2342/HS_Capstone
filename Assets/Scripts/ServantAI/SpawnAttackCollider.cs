using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class SpawnAttackCollider : MonoBehaviourPun
{
    ServantFSM fsm;
    Collider col;

    [SerializeField] float spawnDamage = 20f;   // 스폰 공격력

    void Awake()
    {
        fsm = GetComponentInParent<ServantFSM>();
        if (fsm == null) Debug.LogError("SpawnAttackCollider: 부모에 ServantFSM 이 없습니다!");

        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // 내 소환수의 PhotonView 소유주만 처리
        if (!photonView.IsMine) return;

        // Enemy 태그/레이어만
        if (!other.CompareTag("Enemy")) return;

        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            // ‼️ RPC 직접 호출 X
            dmg.TakeDamage(spawnDamage, transform.position, AttackerType.PinkPlayer);
        }
    }
}
