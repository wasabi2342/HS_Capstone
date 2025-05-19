using UnityEngine;

public class ServantSpawnAttack : MonoBehaviour, IMonsterAttack
{
    [SerializeField] GameObject spawnCollider;   // 콜라이더 오브젝트

    public void SetColliderRight() => ShiftCollider(+1);
    public void SetColliderLeft() => ShiftCollider(-1);
    public void OnAttackAnimationEndEvent() => DisableAttack();

    void ShiftCollider(int dir)     // +1 → 오른쪽, -1 → 왼쪽
    {
        //
    }

    /* IMonsterAttack 구현 */
    public void EnableAttack() => spawnCollider.SetActive(true);
    public void DisableAttack() => spawnCollider.SetActive(false);
    public void Attack(Transform target) { /* 필요시 히트이펙트 */ }
    public void SetDirection(float s)
    {
        if (s >= 0f) SetColliderRight();
        else SetColliderLeft();
    }
}
