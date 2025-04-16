using UnityEngine;

public class GhoulAttack : MonoBehaviour, IMonsterAttack
{
    public int damage = 10;
    private Transform target;
    public GameObject weaponColliderObject;

    public void Attack(Transform target)
    {
        this.target = target;
    }
}
