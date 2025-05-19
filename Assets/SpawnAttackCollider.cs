using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class SpawnAttackCollider : MonoBehaviourPun
{
    ServantFSM fsm;
    PhotonView pv;
    Collider col;
    [SerializeField] float spawnDamage = 20f; // 공격력

    void Awake()
    {
        // 부모에 있는 ServantFSM과 PhotonView 캐싱
        fsm = GetComponentInParent<ServantFSM>();
        if (fsm == null)
            Debug.LogError("ServantAttackCollider: 부모에 ServantFSM이 없습니다!");
        col = GetComponent<Collider>();
        col.isTrigger = true; // Trigger로 설정

        pv = fsm.GetComponent<PhotonView>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 1) 소유주만 처리
        if (!pv.IsMine) return;

        // 2) 태그로 Enemy만 필터
        if (!other.CompareTag("Enemy")) return;

        // 3) IDamageable 찾아서 데미지 RPC
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            // ServantFSM 내 RPC_TakeDamage 호출
            pv.RPC(nameof(ServantFSM.RPC_TakeDamage), RpcTarget.AllBuffered,
                   spawnDamage, pv.ViewID);
        }
    }
}
