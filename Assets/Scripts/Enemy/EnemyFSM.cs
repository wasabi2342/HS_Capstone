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
    private WeaponBase currentWeapon;   // ���� Ȱ��ȭ�� ����

    public void Setup(Transform player, GameObject[] wayPoints)
    {
        this.player = player;

        navMeshAgent = GetComponent<NavMeshAgent>();

        if(navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent ������Ʈ�� �����ϴ�.");
        }
        behaviorAgent = GetComponent<BehaviorGraphAgent>();
        currentWeapon = GetComponent<WeaponBase>();

        navMeshAgent.updateRotation = false;  // �ڵ� ȸ�� ��Ȱ��ȭ
        navMeshAgent.updateUpAxis = false;    // Y�� ȸ�� ��Ȱ��ȭ
        navMeshAgent.angularSpeed = 0f;       // ȸ�� �ӵ��� 0���� ���� (���� ����)

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
        if (navMeshAgent.velocity.sqrMagnitude > 0.01f) // �̵� ���� ���� ȸ�� üũ
        {
            if (navMeshAgent.velocity.x > 0.1f) // ������ �̵�
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (navMeshAgent.velocity.x < -0.1f) // ���� �̵�
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