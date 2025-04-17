using UnityEngine;
using Photon.Pun;

public class SkeletonAttack : MonoBehaviour, IMonsterAttack
{
    [Header("Projectile")]
    public string projectilePrefabName = "ArrowProjectile";   // Resources/ArrowProjectile.prefab
    public Transform firePoint;                               // 활 끝
    public float projectileSpeed = 18f;
    private readonly Vector3 leftAttackOffset = new Vector3(-0.45f, 1f, 0f);
    private readonly Vector3 rightAttackOffset = new Vector3(0.45f, 1f, 0f);

    /// <summary>EnemyAI 가 공격 anim 재생 직전에 호출</summary>
    public void Attack(Transform target)
    {
        cachedTarget = target;          // 실제 발사는 애니메이션 이벤트에서
        Debug.Log($"[SkeletonAttack] Attack() – target = {target?.name}");

    }

    // ─────────────────────────────
    // 애니메이션 이벤트용 메서드 ShootArrow
    // ─────────────────────────────
    public void ShootArrow()
    {
        if (cachedTarget == null || firePoint == null) return;
        Debug.Log($"[SkeletonAttack] ShootArrow 호출 at {Time.time:F2}s");   // ← 임시 로그

        // 방향계산(y 고정)
        Vector3 dir = cachedTarget.position - firePoint.position;
        dir.y = 1f;
        dir.Normalize();

        // 네트워크 투사체 생성
        GameObject proj = PhotonNetwork.Instantiate(
            projectilePrefabName,
            firePoint.position,
            Quaternion.LookRotation(dir));

        // 속도 부여
        if (proj.TryGetComponent(out Rigidbody rb))
            rb.velocity = dir * projectileSpeed;

        // 데미지 전달
        if (proj.TryGetComponent(out ArrowProjectile ap) &&
            TryGetComponent(out EnemyAI ai))
        {
            ap.overrideDamage = ai.status.damage;
        }

        cachedTarget = null;   // 재사용 방지
    }
    public void FirePointLeft()
    {
        if (firePoint == null) return;
        firePoint.localPosition = leftAttackOffset;
    }

    public void FirePointRight()
    {
        if (firePoint == null) return;
        firePoint.localPosition = rightAttackOffset;
    }
    // ─────────────────────────────
    // 내부
    // ─────────────────────────────
    private Transform cachedTarget;
}
