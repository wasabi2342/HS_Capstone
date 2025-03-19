using UnityEngine;

public class TestBullet : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.left * 3 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name} �浹 ����: {other.gameObject.name}, ���̾�: {LayerMask.LayerToName(other.gameObject.layer)}");
        if (other.gameObject.layer == LayerMask.NameToLayer("Damagable"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log("���� �浹 �� ");
                damageable.TakeDamage(10);
            }
        }
    }
}
