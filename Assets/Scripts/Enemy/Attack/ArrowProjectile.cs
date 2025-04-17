using UnityEngine;
using Photon.Pun;

/// <summary>
/// Skeleton 화살 투사체 – 직선 이동·충돌·파괴
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ArrowProjectile : MonoBehaviourPun
{
    public float speed = 0.2f;   // m/s
    public float lifeTime = 4f;    // 초
    public float overrideDamage = -1f; // 음수면 EnemyStatus.damage 사용

    float timer;
    void Awake()
    {
        Debug.Log($"[ArrowProjectile] Spawned by {photonView.Owner.NickName}");
    }

    void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        timer += Time.fixedDeltaTime;
        if (timer >= lifeTime && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        IDamageable dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
            dmg.TakeDamage(overrideDamage > 0 ? overrideDamage : 0f);

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
