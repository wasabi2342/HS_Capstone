using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class ArrowProjectile : MonoBehaviourPun
{
    GameObject owner;
    float damage;

    public void Init(GameObject ownerObj, float dmg,
                     Vector3 dir, float speed, float lifeTime)
    {
        owner = ownerObj;
        damage = dmg;

        if (TryGetComponent(out SpriteRenderer sr))
            sr.flipX = dir.x < 0f;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = dir.normalized * speed;  // x축 ±방향
        transform.forward = dir;

        Destroy(gameObject, lifeTime);           // 안전 파괴
    }

    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;          // 마스터만 판정
        if (other.gameObject == owner) return;   // 자기 자신 무시

        if (other.TryGetComponent(out IDamageable hp))
            hp.TakeDamage(damage, transform.position);

        PhotonNetwork.Destroy(gameObject);
    }
}
