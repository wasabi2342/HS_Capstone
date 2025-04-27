using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class MonsterAttackCollider : MonoBehaviourPun
{
    [Header("Weapon Collider 설정")]
    [SerializeField] private GameObject weaponColliderObject; // WeaponCollider 오브젝트
    [SerializeField] private Collider weaponCollider;         // Collider 공통 타입 (Box/Sphere 다 가능)

    private EnemyAI enemyAI;
    private Vector3 defaultCenter;    // 기본 Center 저장

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogError("MonsterAttackCollider: 부모 객체에서 EnemyAI를 찾을 수 없습니다.");
        }

        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }

        if (weaponCollider != null)
        {
            defaultCenter = weaponCollider switch
            {
                BoxCollider box => box.center,
                SphereCollider sphere => sphere.center,
                _ => Vector3.zero
            };
        }

        if (weaponColliderObject != null)
        {
            weaponColliderObject.SetActive(false);  // 시작 시 비활성화
        }
    }

    // ─────────────────────────────
    // 애니메이션 이벤트용
    // ─────────────────────────────

    public void SetColliderRight()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
        {
            box.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        }
        else if (weaponCollider is SphereCollider sphere)
        {
            sphere.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        }
    }

    public void SetColliderLeft()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
        {
            box.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        }
        else if (weaponCollider is SphereCollider sphere)
        {
            sphere.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        }
    }

    public void EnableCollider()
    {
        if (weaponColliderObject != null)
        {
            weaponColliderObject.SetActive(true);
        }
    }

    public void DisableCollider()
    {
        if (weaponColliderObject != null)
        {
            weaponColliderObject.SetActive(false);
        }
    }

    // ─────────────────────────────
    // 충돌 판정
    // ─────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        //if (!photonView.IsMine) return;
      
        if (other.CompareTag("InteractionZone") || other.gameObject.name.Contains("InteractionZone"))
        {
            return;
        }
    
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && enemyAI != null)
        {
            damageable.TakeDamage(enemyAI.status.damage);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!photonView.IsMine) return;
    //
    //    if (other.CompareTag("InteractionZone") || other.gameObject.name.Contains("InteractionZone"))
    //        return;
    //
    //    PhotonView targetPhotonView = other.GetComponentInParent<PhotonView>();
    //    if (targetPhotonView == null) return;
    //
    //    targetPhotonView.RPC("TakeDamageFromMonster", targetPhotonView.Owner, enemyAI.status.damage);
    //
    //}
}
