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
    [SerializeField] float arrowLife = 20f;

    float facing = 1f;         // +1 ⇢ Right, -1 ⇢ Left
    EnemyFSM fsm;
    public string AnimKey => "Attack1";
    void Awake() => fsm = GetComponent<EnemyFSM>();
    public void SetFirePointRight() => ShiftFirePoint(+1);
    public void SetFirePointLeft() => ShiftFirePoint(-1);

    void ShiftFirePoint(int dir)
    {
        if (!firePoint) return;

        Vector3 lp = firePoint.localPosition;
        lp.x = Mathf.Abs(lp.x) * dir;      // 기준값 × 부호
        firePoint.localPosition = lp;

        /* 화살이 항상 정면(+Z)을 바라보도록 firePoint 자체의
           localRotation Y = 0, Z = (dir==-1 ? 180 : 0) 도 맞춰주면
           SpriterRenderer.flipX 대신 Prefab 회전에만 의존할 수 있음 */
    }
    /* ---------- IMonsterAttack ---------- */
    public void SetDirection(float dir)
    {
        facing = Mathf.Sign(dir);
        ShiftFirePoint((int)facing);       // 방향 바뀔 때마다 위치 갱신
    }
    public void EnableAttack() { }            // 근접형이 아니므로 사용 안함
    public void DisableAttack() { }
    public void Attack(Transform target) { }   // 코루틴 호출 시 아무것도 안함

    /* ---------- Animation Event ---------- */
    // Attack 클립 중간 프레임에 이 함수 이름을 등록
    public void Anim_FireArrow()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        /* ───── 회전값 결정 ───── */
        Quaternion rot = (facing < 0f)
            ? Quaternion.Euler(0f, 0f, 180f)   // 왼쪽  ←  Z축 180°
            : Quaternion.identity;             // 오른쪽 →  Z축   0°

        Vector3 pos = firePoint ? firePoint.position : transform.position;

        /* ───── 네트워크 Instantiate ───── */
        GameObject go = PhotonNetwork.Instantiate(
                            "skeleton_arrow",      // Resources 경로
                            pos,
                            rot);                    // ← 회전 함께 전달

        /* 속도·데미지 초기화 */
        if (go.TryGetComponent(out ArrowProjectile proj))
        {
            Vector3 dir = Vector3.right * facing;    // +X / –X
            proj.Init(gameObject, fsm.EnemyStatusRef.attackDamage,
                      dir, arrowSpeed, arrowLife);
        }
    }

}
