using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Speed 파라미터: 이동 속도
        float speed = agent.velocity.magnitude;
        animator.SetFloat("MoveSpeed", speed);

        // MoveX, MoveY: 이동 방향
        if (speed > 0.01f)
        {
            // 정규화된 방향벡터
            Vector3 dir = agent.velocity.normalized;

            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", -1f); // Idle시 아래 바라보게 한다든지 원하는 방향으로
        }
    }

    public void PlayAttackAnimation(Vector3 targetPos)
    {
        // 공격 방향 계산
        Vector3 dir = (targetPos - transform.position).normalized;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);

        animator.SetBool("IsAttacking", true);
    }

    public void StopAttackAnimation()
    {
        animator.SetBool("IsAttacking", false);
    }

    public void PlayHitAnimation()
    {
        animator.SetBool("IsHit", true);
    }

    public void StopHitAnimation()
    {
        animator.SetBool("IsHit", false);
    }

    public void PlayDeathAnimation()
    {
        animator.SetBool("IsDead", true);
    }
}
