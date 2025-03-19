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
    private float velocityX;
    private WeaponBase currentWeapon;
    private float health = 100f; // ���� ü��

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentWeapon = GetComponent<WeaponBase>();



        InvokeRepeating(nameof(LogDebugInfo), 0f, 3f);
    }

    void Update()
    {
        if (isDead) return; // ��� ���¿����� �ƹ� ���۵� ���� ����
        if (isAttacking) return; // ���� �� �̵� �ִϸ��̼� ���� �� ��

        velocityX = agent.velocity.x;

        if (Mathf.Abs(velocityX) > 0.1f)
        {
            lastDirection = Mathf.Sign(velocityX);
        }

        // �ִϸ����� �Ķ���� ������Ʈ
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
