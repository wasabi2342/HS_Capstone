using UnityEngine;

public class TestEnemy : ParentPlayerController
{
    private void Awake()
    {
        currentHealth = 100f;
    }

    public override void TakeDamage(float damage)
    {

        base.TakeDamage(damage);


        Debug.Log("���� ü��: " + currentHealth);
    }

}