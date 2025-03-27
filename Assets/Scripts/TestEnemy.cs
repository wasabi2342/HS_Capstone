using UnityEngine;

public class TestEnemy : ParentPlayerController
{
    private void Awake()
    {
        //runTimeData.currentHealth = 100f;
    }

    public override void TakeDamage(float damage)
    {

        base.TakeDamage(damage);


        //Debug.Log("몬스터 체력: " + runTimeData.currentHealth);
    }

}