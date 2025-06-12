using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ScarecrowAttackEvent : MonoBehaviour
{
    [Header("Weapon Collider ������Ʈ")]
    [SerializeField] GameObject weaponCollider;   // WeaponCollider GO drag

    void Awake()
    {
        if (weaponCollider) weaponCollider.SetActive(false);   // �⺻ OFF
    }

    /* ������ �ִϸ��̼� �̺�Ʈ���� ȣ�� ������ */
    public void EnableWeaponCollider() => weaponCollider.SetActive(true);
    public void DisableWeaponCollider() => weaponCollider.SetActive(false);
}
