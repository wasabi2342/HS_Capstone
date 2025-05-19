using UnityEngine;
using Photon.Pun;

/// <summary>
/// Skeleton(원거리) 전용 공격 컴포넌트
/// 1) SetDirection() → 현재 바라보는 +/- 방향 기억
/// 2) Anim_FireArrow() ← 애니메이션 이벤트가 호출
///    └ MasterClient 에서만 화살을 네트워크 Instantiate
/// ※ Attack() 은 코루틴 호출 때 중복 발사를 막기 위해 비워 둠
/// </summary>
public class SkeletonAttack : MonoBehaviourPun, IMonsterAttack
{
    [Header("Prefabs / Points")]
    [SerializeField] Transform firePoint;          // 화살 출발 지점
    [SerializeField] GameObject arrowPrefab;       // Resources 혹은 Inspector 등록

    [Header("Projectile")]
    [SerializeField] float arrowSpeed = 8f;
    [SerializeField] float arrowLife = 5f;

    float facing = 1f;         // +1 ⇢ Right, -1 ⇢ Left
    EnemyFSM fsm;

    void Awake() => fsm = GetComponent<EnemyFSM>();

    /* ---------- IMonsterAttack ---------- */
    public void SetDirection(float dir) => facing = Mathf.Sign(dir);
    public void EnableAttack() { }            // 근접형이 아니므로 사용 안함
    public void DisableAttack() { }
    public void Attack(Transform target) { }   // ★ 코루틴 호출 시 아무것도 안함

    /* ---------- Animation Event ---------- */
    // Attack 클립 중간 프레임에 이 함수 이름을 등록
    public void Anim_FireArrow()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 네트워크 Instantiate
        Vector3 pos = firePoint ? firePoint.position : transform.position;
        GameObject go = PhotonNetwork.Instantiate(arrowPrefab.name, pos, Quaternion.identity);

        if (go.TryGetComponent(out ArrowProjectile proj))
        {
            Vector3 dir = Vector3.right * facing;          // x축 ±방향
            proj.Init(gameObject, fsm.EnemyStatusRef.attackDamage,
                      dir, arrowSpeed, arrowLife);
        }
    }
}
