using UnityEngine;
using System.Collections;

public class WolfAttack : MonoBehaviour, IMonsterAttack
{
    public float chargeSpeed = 10f;     // 돌진 속도
    private bool isCharging = false;
    private Transform target;
    public GameObject weaponColliderObject;
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
        // 플레이어 방향 (y 고정, 2D라면 x만 등 상황에 맞게)
        Vector3 direction = (new Vector3(target.position.x, transform.position.y, target.position.z)
                             - startPos).normalized;

        float traveledDistance = 0f;
        float maxChargeDistance = 3f; // 돌진 거리

        // traveledDistance가 5f를 넘지 않는 동안만 이동
        while (traveledDistance < maxChargeDistance)
        {
            float frameMove = chargeSpeed * Time.deltaTime;

            // 혹시 초과 이동하지 않도록 보정
            if (traveledDistance + frameMove > maxChargeDistance)
            {
                frameMove = maxChargeDistance - traveledDistance;
            }

            transform.position += direction * frameMove;
            traveledDistance += frameMove;

            yield return null;
        }

        isCharging = false;
    }


}
