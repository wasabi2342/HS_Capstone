using UnityEngine;

public class WeaponMelee : WeaponBase
{
    [SerializeField] private Collider hitbox; // ���� ��ü ��Ʈ�ڽ�
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackDuration = 0.2f;

    public override void OnAttack()
    {
        Invoke(nameof(ActivateHitbox), 0.1f); // ���� ���� Ȱ��ȭ
        Invoke(nameof(DeactivateHitbox), attackDuration); // ���� ���� ��Ȱ��ȭ
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
            Debug.Log($"���� �������� {other.name}���� {attackDamage} �������� ��!");
        }
    }
}
