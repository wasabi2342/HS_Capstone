using System.Collections;
using UnityEngine;

public class TestDamage : MonoBehaviour
{
    public GameObject bullet;

    private void Start()
    {
        StartCoroutine(FireBullet());
    }

    IEnumerator FireBullet()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            Destroy(Instantiate(bullet), 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("���� �浹 �� ");
        other.GetComponent<IDamageable>().TakeDamage(10);
    }
}
