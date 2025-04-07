using Photon.Pun;
using UnityEngine;

public class TestBullet : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.left * 3 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} 충돌 감지: {other.gameObject.name}, 레이어: {LayerMask.LayerToName(other.gameObject.layer)}");
        Debug.Log("피해 충돌 함 ");
        other.GetComponentInParent<IDamageable>().TakeDamage(10);
    }
}
