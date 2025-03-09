using UnityEngine;

public class TestBullet : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.left * 3 * Time.deltaTime);       
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("���� �浹 �� ");
        other.GetComponentInParent<IDamageable>().TakeDamage(10);
    }
}
