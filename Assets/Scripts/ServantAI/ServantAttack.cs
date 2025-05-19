using UnityEngine;

public class ServantAttack : MonoBehaviour, IMonsterAttack
{
    [SerializeField] GameObject weaponCollider;   // 콜라이더 오브젝트
    [SerializeField] GameObject spawnCollider;

    public void SetColliderRight() => ShiftCollider(+1);
    public void SetColliderLeft() => ShiftCollider(-1);
    public void OnAttackAnimationEndEvent() => DisableAttack();

    void ShiftCollider(int dir)     // +1 → 오른쪽, -1 → 왼쪽
    {
        var box = weaponCollider.GetComponent<BoxCollider>();
        var c = box.center; c.x = Mathf.Abs(c.x) * dir; box.center = c;
    }

    /* IMonsterAttack 구현 */
    public void EnableAttack() => weaponCollider.SetActive(true);
    public void DisableAttack() => weaponCollider.SetActive(false);    
    public void EnableSpawnAttack() => spawnCollider.SetActive(true);
    public void DisableSpawnAttack() => spawnCollider.SetActive(false);
    public void Attack(Transform target) { /* 필요시 히트이펙트 */ }
    public void SetDirection(float s)
    {
        if (s >= 0f) SetColliderRight();
        else SetColliderLeft();
    }
}
