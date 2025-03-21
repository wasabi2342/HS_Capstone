using UnityEngine;

public class GhoulAttack : MonoBehaviour, IMonsterAttack
{
    public int damage = 10;

    public void Attack(Transform target)
    {
        if (target != null)
        {
            //target.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }
}
