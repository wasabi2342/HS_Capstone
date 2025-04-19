using System.Collections.Generic;
using UnityEngine;

public class MonsterStatusManager : MonoBehaviour
{

    public static MonsterStatusManager instance;

    [SerializeField]
    private List<EnemyStatus> enemyStatusList;

    private Dictionary<EnemyStatus, float> originalDamageDict = new Dictionary<EnemyStatus, float>();

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
            originalDamageDict[enemy] = enemy.damage;
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
            enemy.damage *= value;
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
                enemy.damage = originalDamageDict[enemy];
            }
        }
    }
}
