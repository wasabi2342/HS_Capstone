using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Photon.Pun;

public class EnemyStateController : MonoBehaviourPun
{
    [Header("Common")]
    public EnemyStatus baseStatus;
    [HideInInspector]
    public EnemyStatus status;
    private Transform player;
    private NavMeshAgent agent;
    private Vector3 spawnPoint; // 스폰 위치 저장
    public GameObject attackBox; // AttackBox 관리

    /*
    [Header("UI")]
    public GameObject hpBarPrefab;
    public GameObject canvas;
    private Image nowHpBar;
    private RectTransform hpBar;
    private float targetFillAmount;
    */

    private bool isChasing = false;
    public bool isDying = false;

    public void SetSpawnPoint(Vector3 position)
    {
        spawnPoint = position;
    }

    void Start()
    {
        status = Instantiate(baseStatus);
        agent = GetComponent<NavMeshAgent>();
        agent.speed = status.speed;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        attackBox.SetActive(false); // 기본적으로 비활성화
    }

    void Update()
    {
        if (isDying) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float spawnDistance = Vector3.Distance(transform.position, spawnPoint);

        // 플레이어 감지 후 추적 시작
        if (distance <= status.detectionSize.x && !isChasing)
        {
            isChasing = true;
            attackBox.SetActive(true); // 플레이어 감지 시 AttackBox 활성화
            Debug.Log("[추적] 몬스터가 플레이어를 감지하고 추적을 시작합니다.");
        }

        // 추적 중, 범위 내에서는 계속 추적
        if (isChasing)
        {
            if (distance <= status.chaseSize.x) // 추적 범위 내
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else // 추적 범위를 벗어나면 Spawn Point로 복귀
            {
                isChasing = false;
                attackBox.SetActive(false); // 추적 해제 시 AttackBox 비활성화
                agent.isStopped = false;
                agent.SetDestination(spawnPoint);
                Debug.Log("[이동] 몬스터가 스폰포인트로 복귀 중...");
            }
        }

        // Spawn Point로 이동 중, 도착하면 대기 상태로 변경
        if (!isChasing && spawnDistance < 0.5f)
        {
            agent.isStopped = true;
            Debug.Log("[대기] 몬스터가 Spawn Point에 도착하여 대기 상태로 전환.");
        }

        /*
        // 체력 UI 업데이트
        if (hpBar != null)
        {
            nowHpBar.fillAmount = (float)status.hp / baseStatus.hp;
            hpBar.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        }
        */

        // 체력이 0 이하이면 사망
        if (status.hp <= 0 && !isDying)
        {
            Die();
        }
    }

    public void StopMovement()
    {
        agent.isStopped = true; // 공격 시 멈추게 하기
    }

    public void ResumeMovement()
    {
        agent.isStopped = false; // 공격 후 다시 이동
    }

    void Die()
    {
        isDying = true;
        agent.isStopped = true;
        attackBox.SetActive(false); // 사망 시 AttackBox 비활성화
        Debug.Log("[사망] 몬스터가 사망했습니다.");

        Destroy(gameObject, 2f);
    }

    public void TakeDamage(int damage)
    {
        photonView.RPC("UpdateHP", RpcTarget.All, damage);
    }

    [PunRPC]
    protected void UpdateHP(int damage)
    {
        status.hp -= damage;
    }
}
