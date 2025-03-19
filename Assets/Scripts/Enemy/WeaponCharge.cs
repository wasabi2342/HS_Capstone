using UnityEngine;
using UnityEngine.AI;

public class WeaponCharge : WeaponBase
{
    [SerializeField] private float chargeSpeed = 10f; // ���� �ӵ�
    [SerializeField] private float chargeDuration = 1.5f; // ���� �ð�
    [SerializeField] private float attackDamage = 15f; // ���� ���ݷ�
    private NavMeshAgent agent;
    private bool isCharging = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnAttack()
    {
        if (!isCharging)
        {
            StartCoroutine(ChargeAttack());
        }
    }

    private System.Collections.IEnumerator ChargeAttack()
    {
        isCharging = true;
        float originalSpeed = agent.speed;
        agent.speed = chargeSpeed;

        float startTime = Time.time;
        while (Time.time - startTime < chargeDuration)
        {
            agent.destination = target.position; // ��ǥ �������� ����
            yield return null;
        }

        agent.speed = originalSpeed;
        isCharging = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCharging && other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            Debug.Log($"���� �������� {other.name}���� {attackDamage} �������� ��!");
            isCharging = false;
        }
    }
}
