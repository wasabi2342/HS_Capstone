using UnityEngine;
using System.Collections;

public class WolfAttack : MonoBehaviour, IMonsterAttack
{
    [Header("Charge Settings")]
    [Tooltip("돌진 속도 (m/s)")]
    public float chargeSpeed = 10f;

    [Tooltip("돌진 최대 거리 (m)")]
    public float maxChargeDistance = 3f;

    [Header("공격 콜라이더")]
    public GameObject weaponColliderObject;

    // ───────── 내부 캐시 ─────────
    private Collider weaponCollider;
    private Vector3 defaultCenter;
    private bool isCharging;
    private Transform chargeTarget;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (weaponColliderObject == null)
        {
            Debug.LogError("[WolfAttack] weaponColliderObject가 할당되지 않았습니다.", this);
            return;
        }

        weaponCollider = weaponColliderObject.GetComponent<Collider>();
        if (weaponCollider == null)
        {
            Debug.LogError("[WolfAttack] WeaponCollider 오브젝트에 Collider 컴포넌트가 없습니다.", weaponColliderObject);
        }
        else
        {
            // Collider 종류별로 기본 center 저장
            if (weaponCollider is BoxCollider bc) defaultCenter = bc.center;
            if (weaponCollider is SphereCollider sc) defaultCenter = sc.center;
            if (weaponCollider is CapsuleCollider cc) defaultCenter = cc.center;
        }

        // 기본 비활성화
        weaponColliderObject.SetActive(false);
    }

    /// <summary>
    /// FSM AttackState 에서 호출 (윈드업 후에)
    /// 플레이어 방향으로 돌진을 시작합니다.
    /// </summary>
    public void Attack(Transform target)
    {
        if (isCharging || target == null) return;
        isCharging = true;
        chargeTarget = target;
        StartCoroutine(ChargeAttackRoutine());
    }

    private IEnumerator ChargeAttackRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(chargeTarget.position.x, transform.position.y, chargeTarget.position.z);
        Vector3 dir = (targetPos - startPos).normalized;

        float traveled = 0f;
        while (traveled < maxChargeDistance)
        {
            float move = chargeSpeed * Time.deltaTime;
            if (traveled + move > maxChargeDistance)
                move = maxChargeDistance - traveled;

            transform.position += dir * move;
            traveled += move;
            yield return null;
        }

        isCharging = false;
    }

    /// <summary>애니메이션 이벤트: 콜라이더 켜기</summary>
    public void EnableAttack()
    {
        if (weaponColliderObject) weaponColliderObject.SetActive(true);
    }

    /// <summary>애니메이션 이벤트: 콜라이더 끄기</summary>
    public void DisableAttack()
    {
        if (weaponColliderObject) weaponColliderObject.SetActive(false);
    }

    /// <summary>애니메이션 이벤트: 오른쪽 공격 시 호출</summary>
    public void SetColliderRight()
    {
        if (weaponCollider == null) return;
        Vector3 c = defaultCenter;
        c.x = Mathf.Abs(defaultCenter.x);

        if (weaponCollider is BoxCollider bc) bc.center = c;
        if (weaponCollider is SphereCollider sc) sc.center = c;
        if (weaponCollider is CapsuleCollider cc) cc.center = c;
    }

    /// <summary>애니메이션 이벤트: 왼쪽 공격 시 호출</summary>
    public void SetColliderLeft()
    {
        if (weaponCollider == null) return;
        Vector3 c = defaultCenter;
        c.x = -Mathf.Abs(defaultCenter.x);

        if (weaponCollider is BoxCollider bc) bc.center = c;
        if (weaponCollider is SphereCollider sc) sc.center = c;
        if (weaponCollider is CapsuleCollider cc) cc.center = c;
    }

    /// <summary>
    /// FSM AttackState.Execute 에서 방향(+1/-1) 전달용
    /// </summary>
    public void SetDirection(float sign)
    {
        if (sign >= 0f) SetColliderRight();
        else SetColliderLeft();
    }

    /// <summary>애니메이션 이벤트: 공격 애니 끝날 때 호출</summary>
    public void OnAttackAnimationEndEvent()
    {
        DisableAttack();
    }
}
