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

    [Tooltip("공격 콜라이더 최대 유지 시간 (초)")]
    public float maxColliderActiveTime = 0.5f; // 콜라이더 활성화 최대 시간

    private Collider weaponCollider;
    private Vector3 defaultCenter;
    private bool isCharging;
    private float colliderActiveTimer = 0f;
    private bool isColliderActive = false;

    private float attackDirection = 1f; // +1 오른쪽 / -1 왼쪽
    private EnemyFSM fsm;
    private Animator animator;
    public string AnimKey => "Attack1";
    public float WindUpRate => 0.60f;
    void Awake()
    {
        animator = GetComponent<Animator>();
        fsm = GetComponent<EnemyFSM>();

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
            if (weaponCollider is BoxCollider bc) defaultCenter = bc.center;
            if (weaponCollider is SphereCollider sc) defaultCenter = sc.center;
            if (weaponCollider is CapsuleCollider cc) defaultCenter = cc.center;
        }

        weaponColliderObject.SetActive(false);
    }

    void Update()
    {
        if (isColliderActive)
        {
            colliderActiveTimer += Time.deltaTime;
            if (colliderActiveTimer >= maxColliderActiveTime)
            {
                Debug.LogWarning("[WolfAttack] Attack collider was active too long. Forcing disable.");
                DisableAttack();
            }
        }
    }

    public void Attack(Transform target)
    {
        if (isCharging) return;
        isCharging = true;
        StartCoroutine(ChargeAttackRoutine());
    }

    private IEnumerator ChargeAttackRoutine()
    {
        // NavMeshAgent 위치 갱신 막기
        if (fsm.Agent != null)
        {
            fsm.Agent.isStopped = true;
            fsm.Agent.updatePosition = false;
        }

        Vector3 dir = (attackDirection >= 0f) ? Vector3.right : Vector3.left;
        Vector3 startPos = transform.position;

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

        // 돌진 후 위치를 NavMeshAgent에도 강제 동기화
        if (fsm.Agent != null)
        {
            fsm.Agent.Warp(transform.position);
            fsm.Agent.updatePosition = true;
            fsm.Agent.isStopped = false;
        }
    }

    public void EnableAttack()
    {
        if (weaponColliderObject)
        {
            weaponColliderObject.SetActive(true);
            isColliderActive = true;
            colliderActiveTimer = 0f;
            Debug.Log("[WolfAttack] Attack collider enabled");
        }
    }

    public void DisableAttack()
    {
        if (weaponColliderObject)
        {
            weaponColliderObject.SetActive(false);
            isColliderActive = false;
            Debug.Log("[WolfAttack] Attack collider disabled");
        }
    }

    public void SetColliderRight()
    {
        if (weaponCollider == null) return;
        Vector3 c = defaultCenter;
        c.x = Mathf.Abs(defaultCenter.x);
        SetColliderCenter(c);
    }

    public void SetColliderLeft()
    {
        if (weaponCollider == null) return;
        Vector3 c = defaultCenter;
        c.x = -Mathf.Abs(defaultCenter.x);
        SetColliderCenter(c);
    }

    private void SetColliderCenter(Vector3 center)
    {
        if (weaponCollider is BoxCollider bc) bc.center = center;
        if (weaponCollider is SphereCollider sc) sc.center = center;
        if (weaponCollider is CapsuleCollider cc) cc.center = center;
    }

    public void SetDirection(float sign)
    {
        attackDirection = sign;
        if (sign >= 0f) SetColliderRight();
        else SetColliderLeft();
    }

    public void OnAttackAnimationEndEvent()
    {
        DisableAttack();
        Debug.Log("[WolfAttack] Attack animation ended");
    }
}
