using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private MovementRigidbody2D movement;
    private Transform target;
    private float damage;

    public void Setup(Transform target, float damage)
    {
        movement = GetComponent<MovementRigidbody2D>();
        this.target = target;
        this.damage = damage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

