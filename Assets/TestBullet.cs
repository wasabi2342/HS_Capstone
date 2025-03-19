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
        if (other.gameObject.layer == LayerMask.NameToLayer("Damagable"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log("피해 충돌 함 ");
                damageable.TakeDamage(10);
            }
        }
    }
}
