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


        Debug.Log("몬스터 체력: " + currentHealth);
    }

}