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

        if(navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent 컴포넌트가 없습니다.");
        }
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        currentWeapon = GetComponent<WeaponBase>();

        navMeshAgent.updateRotation = false;  // 자동 회전 비활성화
        navMeshAgent.updateUpAxis = false;    // Y축 회전 비활성화
        navMeshAgent.angularSpeed = 0f;       // 회전 속도를 0으로 설정 (완전 고정)

        behaviorAgent.SetVariableValue("PatrolPoints", wayPoints.ToList());
        behaviorAgent.SetVariableValue("player", player.gameObject);

        currentWeapon.Setup(player, damage, cooldowmTime);
    }
    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (navMeshAgent == null) return;
        if (navMeshAgent.velocity.sqrMagnitude > 0.01f) // 이동 중일 때만 회전 체크
        {
            if (navMeshAgent.velocity.x > 0.1f) // 오른쪽 이동
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (navMeshAgent.velocity.x < -0.1f) // 왼쪽 이동
            {
                transform.rotation = Quaternion.Euler(180, 0, 180);
            }
        }
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