using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ScarecrowAttackEvent : MonoBehaviour
{
    [Header("Weapon Collider 오브젝트")]
    [SerializeField] GameObject weaponCollider;   // WeaponCollider GO drag

    void Awake()
    {
        if (weaponCollider) weaponCollider.SetActive(false);   // 기본 OFF
    }

    /* ─── 애니메이션 이벤트에서 호출 ─── */
    public void EnableWeaponCollider() => weaponCollider.SetActive(true);
    public void DisableWeaponCollider() => weaponCollider.SetActive(false);
}
