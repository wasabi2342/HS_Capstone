using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WeaponCharge : WeaponBase
{
    [SerializeField] private float chargeSpeed = 10f; // ���� �ӵ�
    [SerializeField] private float attackDamage = 15f; // ���� ������
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
            Debug.Log($"[{gameObject.name}] ���� ����! �ִϸ��̼� ����.");

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

    // �ִϸ��̼� �̺�Ʈ���� ȣ�� (���� ����)
    public void StartCharge()
    {
        if (!isCharging && target != null)
        {
            isCharging = true;
            agent.speed = chargeSpeed;

            // ���� ���� ��� (�÷��̾� �������� 2f �̵�)
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 chargeDestination = transform.position + (direction * 2f);

            // NavMesh�� ����Ͽ� ����
            agent.SetDestination(chargeDestination);
            Debug.Log($"[{gameObject.name}] ���� ����! ��ǥ ��ġ: {chargeDestination}");
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ�� (���� ����)
    public void StopCharge()
    {
        isCharging = false;
        agent.speed = originalSpeed;
        agent.ResetPath(); // ��ǥ ���� �����Ͽ� ����
        Debug.Log($"[{gameObject.name}] ���� ����. ���� �ӵ��� ����.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCharging && other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            Debug.Log($"[{gameObject.name}] ���� �������� {other.name}���� {attackDamage} �������� ��!");
            StopCharge(); // �浹 �� ���� ����
        }
    }

    public void SetFacingDirection(bool isRight)
    {
        isFacingRight = isRight;
    }
}
