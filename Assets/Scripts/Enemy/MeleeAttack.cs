using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttack : MonoBehaviour
{
    [Header("Common")]
    public Transform player;
    public EnemyStatus status;
    public LayerMask AttackableLayer;

    private bool canAttack = true;

    [Header("Hit Box")]
    public GameObject attackBox;
    private Collider attackCollider;

    private NavMeshAgent agent;
    private EnemyStateController stateController;

    public static MeleeAttack instance; // 싱글톤 패턴 추가

    void Awake()
    {
        instance = this; // 싱글톤 초기화
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        stateController = GetComponent<EnemyStateController>();
        attackCollider = attackBox.GetComponent<Collider>();

        if (attackCollider == null)
        {
            Debug.LogError("[오류] AttackBox에 Collider가 없습니다!");
        }
    }

    void Start()
    {
        attackBox.SetActive(false); // 기본적으로 비활성화
    }

    void Update()
    {
        if (stateController.isDying) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= status.attackSize.x)
        {
            Debug.Log($"[공격 감지] 몬스터가 공격 범위 내에 플레이어를 감지 (거리: {distance})");
        }

        if (distance <= status.attackSize.x && canAttack)
        {
            Debug.Log("[공격] 몬스터가 공격을 시도합니다!");
            AttackTrigger();
        }
    }

    void AttackTrigger()
    {
        Debug.Log("[공격] " + status.name + " 공격 실행!");

        switch (status.id)
        {
            case 1: // Ghoul 공격 패턴
                StartCoroutine(AttackRoutineGhoul());
                break;
            case 2: // 다른 몬스터의 공격 패턴 (추후 추가 가능)
                StartCoroutine(AttackRoutineGhoul()); // 추가 몬스터 공격 방식 지정 가능
                break;
            default:
                Debug.LogWarning("[경고] 해당 몬스터 ID에 대한 공격 방식이 지정되지 않음!");
                break;
        }
    }

    IEnumerator AttackRoutineGhoul()
    {
        Debug.Log("[공격] Ghoul이 공격을 준비 중...");
        canAttack = false;
        agent.isStopped = true;

        yield return new WaitForSeconds(status.waitCool);


        yield return new WaitForSeconds(0.2f);


        agent.isStopped = false;
        yield return new WaitForSeconds(status.attackCool);

        canAttack = true;
    }

    public void GiveDamage(Collider target)
    {
        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(status.dmg);
            Debug.Log($"[피격] 플레이어가 {status.dmg}의 피해를 입음!");
        }
        else
        {
            Debug.Log("[피격] 타겟이 플레이어가 아닙니다!");
        }
    }
}
