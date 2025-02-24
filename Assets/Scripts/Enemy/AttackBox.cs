using UnityEngine;
using System.Collections;

public class AttackBox : MonoBehaviour
{
    private MeleeAttack melee;

    void Start()
    {
        melee = gameObject.GetComponentInParent<MeleeAttack>();
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            melee.GiveDamage();
        }
    }
    /*public void OnAttackAnimationEnd()
    {
        gameObject.SetActive(false);
    }
    */
}
