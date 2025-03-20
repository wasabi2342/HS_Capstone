using UnityEngine;
using UnityEngine.AI;

public class MonsterAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private float lastDirection = 1f;
    private bool isAttacking = false;
    private bool isDead = false; // 사망 여부 체크
    private bool isHit = false; // 피격 여부 체크
    private float velocityMagnitude;
    private WeaponBase currentWeapon;
    private float health = 100f; // 몬스터 체력

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentWeapon = GetComponent<WeaponBase>();
    }

    void Update()
    {
        if (isDead) return; // 사망 상태에서는 아무 동작도 하지 않음

        // 이동 속도 가져오기 (NavMeshAgent는 velocity.x 값이 항상 0일 수 있음)
        velocityMagnitude = agent.velocity.magnitude;

        // 방향 업데이트 (속도가 0이 아닐 때만 갱신)
        if (velocityMagnitude > 0.1f)
        {
            lastDirection = Mathf.Sign(agent.velocity.x);
        }

        // 애니메이터 파라미터 업데이트
        animator.SetFloat("velocityX", velocityMagnitude);
        animator.SetFloat("lastDirection", lastDirection);
        animator.SetBool("isMoving", velocityMagnitude > 0.1f);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isHit", isHit);
        animator.SetBool("isDead", isDead);
    }

    public void StartAttack()
    {
        if (isDead) return;

        isAttacking = true;
        animator.SetBool("isAttacking", true);

        // 공격 애니메이션이 끝나면 다시 이동 가능하게 설정
        Invoke(nameof(ResetAttack), 1.0f); // 공격 애니메이션 길이에 맞게 조정
    }

    private void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // 이미 죽은 상태라면 무시

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            HitReaction();
        }
    }

    private void HitReaction()
    {
        if (isHit) return; // 연속 피격 방지
        isHit = true;
        animator.SetBool("isHit", true);
        Invoke(nameof(ResetHit), 0.5f); // 0.5초 후 피격 상태 해제
    }

    private void ResetHit()
    {
        isHit = false;
        animator.SetBool("isHit", false);
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true; // 네비메시 이동 중지
        animator.SetBool("isDead", true);
    }
}
