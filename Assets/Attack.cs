using Photon.Pun;
using UnityEngine;

public class Attack : MonoBehaviourPun
{
    private EnemyAI enemyAI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && enemyAI != null)
        {
            damageable.TakeDamage(enemyAI.status.damage);
        }
    }
}
