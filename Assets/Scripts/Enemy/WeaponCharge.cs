using UnityEngine;
using UnityEngine.AI;

public class WeaponCharge : WeaponBase
{
    [SerializeField] private float chargeSpeed = 10f; // 돌진 속도
    [SerializeField] private float chargeDuration = 1.5f; // 돌진 시간
    [SerializeField] private float attackDamage = 15f; // 돌진 공격력
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
            agent.destination = target.position; // 목표 방향으로 돌진
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
            Debug.Log($"돌진 공격으로 {other.name}에게 {attackDamage} 데미지를 줌!");
            isCharging = false;
        }
    }
}
