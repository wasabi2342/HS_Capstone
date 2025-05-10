using System.Collections;
using UnityEngine;

public class TrapDmageArea : MonoBehaviour
{
    public float damageAmount = 10f;
    public float delayBeforeDamage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ãæµ¹ °¨ÁöµÊ: " + other.name);
        StartCoroutine(DelayedDamage(other));
    }

    IEnumerator DelayedDamage(Collider other)
    {
        yield return new WaitForSeconds(delayBeforeDamage);

        if (other != null)
        {
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                
                damageable.TakeDamage(damageAmount, transform.position);
            }
            else
            {
                
            }
        }
    }
}
