using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WeaponCharge : WeaponBase
{
    [SerializeField] private float chargeSpeed = 10f; // 돌진 속도
    [SerializeField] private float attackDamage = 15f; // 돌진 데미지
    private NavMeshAgent agent;
    private Animator animator;
    private bool isCharging = false;
    private float originalSpeed;
    private bool isFacingRight = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        originalSpeed = agent.speed;
    }

    public override void OnAttack()
    {
        if (!isCharging && animator != null && target != null)
        {
            Debug.Log($"[{gameObject.name}] 공격 시작! 애니메이션 실행.");

            if (isFacingRight)
            {
                animator.Play("Attack_Right");
            }
            else
            {
                animator.Play("Attack_Left");
            }
        }
    }

    // 애니메이션 이벤트에서 호출 (돌진 시작)
    public void StartCharge()
    {
        if (!isCharging && target != null)
        {
            isCharging = true;
            agent.speed = chargeSpeed;

            // 돌진 방향 계산 (플레이어 방향으로 2f 이동)
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 chargeDestination = transform.position + (direction * 2f);

            // NavMesh를 사용하여 돌진
            agent.SetDestination(chargeDestination);
            Debug.Log($"[{gameObject.name}] 돌진 시작! 목표 위치: {chargeDestination}");
        }
    }

    // 애니메이션 이벤트에서 호출 (돌진 종료)
    public void StopCharge()
    {
        isCharging = false;
        agent.speed = originalSpeed;
        agent.ResetPath(); // 목표 지점 제거하여 멈춤
        Debug.Log($"[{gameObject.name}] 돌진 종료. 원래 속도로 복귀.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCharging && other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            Debug.Log($"[{gameObject.name}] 돌진 공격으로 {other.name}에게 {attackDamage} 데미지를 줌!");
            StopCharge(); // 충돌 후 돌진 종료
        }
    }

    public void SetFacingDirection(bool isRight)
    {
        isFacingRight = isRight;
    }
}
