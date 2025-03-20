using UnityEngine;
using UnityEngine.AI;

public class MonsterAnimationController : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private float lastDirection = 1f;
    private bool isAttacking = false;
    private bool isDead = false; // ��� ���� üũ
    private bool isHit = false; // �ǰ� ���� üũ
    private float velocityMagnitude;
    private WeaponBase currentWeapon;
    private float health = 100f; // ���� ü��

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentWeapon = GetComponent<WeaponBase>();
    }

    void Update()
    {
        if (isDead) return; // ��� ���¿����� �ƹ� ���۵� ���� ����

        // �̵� �ӵ� �������� (NavMeshAgent�� velocity.x ���� �׻� 0�� �� ����)
        velocityMagnitude = agent.velocity.magnitude;

        // ���� ������Ʈ (�ӵ��� 0�� �ƴ� ���� ����)
        if (velocityMagnitude > 0.1f)
        {
            lastDirection = Mathf.Sign(agent.velocity.x);
        }

        // �ִϸ����� �Ķ���� ������Ʈ
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

        // ���� �ִϸ��̼��� ������ �ٽ� �̵� �����ϰ� ����
        Invoke(nameof(ResetAttack), 1.0f); // ���� �ִϸ��̼� ���̿� �°� ����
    }

    private void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // �̹� ���� ���¶�� ����

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
        if (isHit) return; // ���� �ǰ� ����
        isHit = true;
        animator.SetBool("isHit", true);
        Invoke(nameof(ResetHit), 0.5f); // 0.5�� �� �ǰ� ���� ����
    }

    private void ResetHit()
    {
        isHit = false;
        animator.SetBool("isHit", false);
    }

    private void Die()
    {
        isDead = true;
        agent.isStopped = true; // �׺�޽� �̵� ����
        animator.SetBool("isDead", true);
    }
}
