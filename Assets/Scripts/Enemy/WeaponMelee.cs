using UnityEngine;

public class WeaponMelee : WeaponBase
{
    [SerializeField] private Collider hitbox; // 몬스터 자체 히트박스
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackDuration = 0.2f;

    public override void OnAttack()
    {
        Invoke(nameof(ActivateHitbox), 0.1f); // 공격 판정 활성화
        Invoke(nameof(DeactivateHitbox), attackDuration); // 공격 판정 비활성화
    }

    private void ActivateHitbox()
    {
        hitbox.enabled = true;
    }

    private void DeactivateHitbox()
    {
        hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            Debug.Log($"근접 공격으로 {other.name}에게 {attackDamage} 데미지를 줌!");
        }
    }
}
