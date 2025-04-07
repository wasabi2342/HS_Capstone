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
        Debug.Log($"{gameObject.name} �浹 ����: {other.gameObject.name}, ���̾�: {LayerMask.LayerToName(other.gameObject.layer)}");
        Debug.Log("���� �浹 �� ");
        other.GetComponentInParent<IDamageable>().TakeDamage(10);
    }
}
