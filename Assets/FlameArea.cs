using Photon.Pun;
using System.Collections;
using UnityEngine;

public class FlameArea : MonoBehaviourPun
{
    [SerializeField]
    private float damage;

    public void Init(float damage, float duration)
    {
        this.damage = damage;
        StartCoroutine(DestroyAfterduration(duration));
    }

    private IEnumerator DestroyAfterduration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform.position);
            }
        }
    }
}
