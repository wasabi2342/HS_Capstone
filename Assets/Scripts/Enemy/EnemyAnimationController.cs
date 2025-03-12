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
        // Speed �Ķ����: �̵� �ӵ�
        float speed = agent.velocity.magnitude;
        animator.SetFloat("MoveSpeed", speed);

        // MoveX, MoveY: �̵� ����
        if (speed > 0.01f)
        {
            // ����ȭ�� ���⺤��
            Vector3 dir = agent.velocity.normalized;

            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }
        else
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", -1f); // Idle�� �Ʒ� �ٶ󺸰� �Ѵٵ��� ���ϴ� ��������
        }
    }

    public void PlayAttackAnimation(Vector3 targetPos)
    {
        // ���� ���� ���
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
