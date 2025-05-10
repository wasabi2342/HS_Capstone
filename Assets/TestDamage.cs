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
            Destroy(Instantiate(bullet, transform.position + Vector3.up, Quaternion.identity), 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("피해 충돌 함 ");
        //other.GetComponent<IDamageable>().TakeDamage(10);
    }
}
