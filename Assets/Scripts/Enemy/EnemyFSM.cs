using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

public class EnemyFSM : MonoBehaviour
{
    [SerializeField]
    private float cooldowmTime = 2f;
    [SerializeField]
    private float damage = 10f;

    private Transform player;
    private NavMeshAgent navMeshAgent;
    private BehaviorGraphAgent behaviorAgent;
    private WeaponBase currentWeapon;   // 현재 활성화된 무기

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        navMeshAgent = GetComponent<NavMeshAgent>();
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        currentWeapon = GetComponent<WeaponBase>();

        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        behaviorAgent.SetVariableValue("PatrolPoints", wayPoints.ToList());
        behaviorAgent.SetVariableValue("player", player.gameObject);

        currentWeapon.Setup(player, damage, cooldowmTime);
    }
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}