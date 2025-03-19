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
    private float velocityX;
    private WeaponBase currentWeapon;
    private float health = 100f; // 몬스터 체력

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentWeapon = GetComponent<WeaponBase>();



        InvokeRepeating(nameof(LogDebugInfo), 0f, 3f);
    }

    void Update()
    {
        if (isDead) return; // 사망 상태에서는 아무 동작도 하지 않음
        if (isAttacking) return; // 공격 중 이동 애니메이션 변경 안 함

        velocityX = agent.velocity.x;

        if (Mathf.Abs(velocityX) > 0.1f)
        {
            lastDirection = Mathf.Sign(velocityX);
        }

        // 애니메이터 파라미터 업데이트
        animator.SetFloat("velocityX", velocityX);
        animator.SetFloat("lastDirection", lastDirection);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isHit", isHit);
        animator.SetBool("isDead", isDead);
    }

    void LogDebugInfo()
    {
        Debug.Log($"[DEBUG] velocityX: {velocityX}, lastDirection: {lastDirection}, isAttacking: {isAttacking}, isDead: {isDead}");
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
