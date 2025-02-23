using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttack : MonoBehaviour
{
    [Header("Common")]
    public Transform player;
    public EnemyStatus status;

    private bool canAttack = true;

    [Header("Hit Box")]
    public GameObject attackBox;
    private Collider attackCollider;

    private NavMeshAgent agent;
    private EnemyStateController stateController;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        stateController = GetComponent<EnemyStateController>();
        attackCollider = attackBox.GetComponent<Collider>();

        if (attackCollider == null)
        {
            Debug.LogError("AttackBox에 Collider가 없습니다!");
        }
    }

    void Start()
    {
        attackBox.SetActive(false);
    }

    void Update()
    {
        if (stateController.isDying || !player) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= status.attackSize.x && canAttack)
        {
            AttackTrigger();
        }
    }

    void AttackTrigger()
    {
        switch (status.id)
        {
            case 1:
                StartCoroutine(AttackRoutineGhoul());
                break;

            case 2: // Wolf의 돌진 공격
                StartCoroutine(AttackRoutineWolf());
                break;

            default:
                Debug.LogWarning("지정되지 않은 몬스터 ID!");
                break;
        }
    }

    IEnumerator AttackRoutineGhoul()
    {
        canAttack = false;
        agent.isStopped = true;

        yield return new WaitForSeconds(status.waitCool);

        attackBox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackBox.SetActive(false);

        yield return new WaitForSeconds(status.attackCool);
        agent.isStopped = false;
        canAttack = true;
    }

    IEnumerator AttackRoutineWolf()
    {
        canAttack = false;

        // 공격 준비 모션 (waitCool 동안 정지)
        agent.isStopped = true;
        yield return new WaitForSeconds(status.waitCool);

        // 플레이어 방향 설정 (돌진 직전에 방향 재설정)
        Vector3 targetDir = (player.position - transform.position).normalized;
        Vector3 targetPos = transform.position + targetDir * status.attackSize.x * 1.5f;

        float dashSpeed = status.speed * 3f;
        float dashDuration = 0.5f;
        float elapsedTime = 0f;

        agent.updatePosition = false;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agent.Warp(transform.position);
        agent.updatePosition = true;
        agent.ResetPath(); // ⭐️ 목적지 초기화 ⭐️

        attackBox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        attackBox.SetActive(false);

        yield return new WaitForSeconds(status.attackCool);

        agent.isStopped = false;
        canAttack = true;
    }


    public void GiveDamage()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(status.dmg);
            Debug.Log($"플레이어가 {status.dmg}의 피해를 입었습니다!");
        }
    }
}
