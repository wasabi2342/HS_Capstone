using System.Collections.Generic;
using UnityEngine;

public class MonsterStatusManager : MonoBehaviour
{
    
    public static MonsterStatusManager instance;

    [SerializeField]
    private List<EnemyStatusSO> enemyStatusList;

    private Dictionary<EnemyStatusSO, float> originalDamageDict = new Dictionary<EnemyStatusSO, float>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        foreach (var enemy in enemyStatusList)
        {
            originalDamageDict[enemy] = enemy.attackDamage;
        }
    }

    /// <summary>
    /// 몬스터의 데미지 * vlaue
    /// </summary>
    /// <param name="value"></param>
    public void EnemyDamageBuff(float value)
    {
        foreach (var enemy in enemyStatusList)
        {
            enemy.attackDamage *= value;
        }
    }

    /// <summary>
    /// 원래 데미지로 복원
    /// </summary>
    public void ResetEnemyDamage()
    {
        foreach (var enemy in enemyStatusList)
        {
            if (originalDamageDict.ContainsKey(enemy))
            {
                enemy.attackDamage = originalDamageDict[enemy];
            }
        }
    }
}
