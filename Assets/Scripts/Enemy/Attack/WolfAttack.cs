using UnityEngine;
using System.Collections;

public class WolfAttack : MonoBehaviour, IMonsterAttack
{
    public float chargeSpeed = 10f;     // 돌진 속도

    private bool isCharging = false;
    private Transform target;
    [Header("공격 콜라이더")]
    public GameObject weaponColliderObject;
    private Collider weaponCollider;
    private Vector3 defaultCenter;

    private Animator animator;

    public void Attack(Transform target)
    {
        animator = GetComponent<Animator>();

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

    // 애니메이션 이벤트용
    public void SetAnimSpeed(float speed)
    {
        animator.speed = 0.8f;
    }
    public void ResetAnimSpeed()
    {
        animator.speed = 1f;
    }
    public void EnableAttack()
    {
        if (weaponColliderObject != null)
            weaponColliderObject.SetActive(true);
    }

    public void DisableAttack()
    {
        if (weaponColliderObject != null)
            weaponColliderObject.SetActive(false);
    }

    public void SetColliderRight()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
            box.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        else if (weaponCollider is SphereCollider sphere)
            sphere.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
    }

    public void SetColliderLeft()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
            box.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        else if (weaponCollider is SphereCollider sphere)
            sphere.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
    }
    public void SetDirection(float sign)
    {
        if (sign > 0f) SetColliderRight();
        else SetColliderLeft();
    }
}
