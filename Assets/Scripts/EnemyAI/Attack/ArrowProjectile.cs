using UnityEngine;
using Photon.Pun;

/// <summary>
/// Skeleton 화살 프로젝타일
///  ─ MasterClient(PhotonView.IsMine)만 판정
///  ─ Player  Servant 레이어에만 데미지 전달
///  ─ 충돌 후 즉시 파괴
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class ArrowProjectile : MonoBehaviourPun
{
    GameObject owner;            // 쏜 Skeleton
    float damage;                // EnemyStatusSO.attackDamage
    [SerializeField] LayerMask victimMask;
    /* ───── 초기화 ───── */
    public void Init(GameObject ownerObj, float dmg,
                     Vector3 dir, float speed, float lifeTime)
    {
        owner = ownerObj;
        damage = dmg;

        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = dir.normalized * speed;

        /* 오른쪽(0°) / 왼쪽(180°) 회전 고정 */
        transform.rotation = (dir.x < 0)
            ? Quaternion.Euler(0, 0, 180)
            : Quaternion.identity;

        Destroy(gameObject, lifeTime);
    }

    /* ───── 충돌 판정 ───── */
    void OnTriggerEnter(Collider other)
    {
        /* MasterClient만 데미지 계산 */
        if (!photonView.IsMine) return;

        /* 자기 자신(Skeleton) 무시 */
        if (other.gameObject == owner) return;

        /* 피해를 줄 레이어인가? */
        if ((victimMask.value & (1 << other.gameObject.layer)) == 0) return;

        /* IDamageable 찾고 데미지 적용 */
        if (other.TryGetComponent(out IDamageable hp))
            hp.TakeDamage(damage, transform.position);

        PhotonNetwork.Destroy(gameObject);      // 화살 파괴 (모두에게 동기화)
    }
}
