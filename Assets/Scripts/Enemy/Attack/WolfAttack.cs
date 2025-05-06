using UnityEngine;
using System.Collections;

public class WolfAttack : MonoBehaviour, IMonsterAttack
{
    public float chargeSpeed = 10f;     // 돌진 속도

    private bool isCharging = false;
    private Transform target;
    [Header("공격 콜라이더")]
    [SerializeField] GameObject weaponCollider;

    public void Attack(Transform target)
    {

        if (!isCharging)
        {
            isCharging = true;
            this.target = target;
            StartCoroutine(ChargeAttack());
        }
    }

    private IEnumerator ChargeAttack()
    {
        Vector3 startPos = transform.position;
        Vector3 direction = (new Vector3(target.position.x, transform.position.y, target.position.z) - startPos).normalized;
        float traveledDistance = 0f;
        float maxChargeDistance = 3f; // 돌진 거리

        while (traveledDistance < maxChargeDistance)
        {
            float frameMove = chargeSpeed * Time.deltaTime;
            if (traveledDistance + frameMove > maxChargeDistance)
                frameMove = maxChargeDistance - traveledDistance;

            transform.position += direction * frameMove;
            traveledDistance += frameMove;
            yield return null;
        }

        isCharging = false;
    }

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
    public void SetDirection(float s) { /* AttackState에서 미리 호출 */ }
}
