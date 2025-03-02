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
    private Vector3 spawnPoint; // ���� ��ġ ����
    public GameObject attackBox; // AttackBox ����

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

        attackBox.SetActive(false); // �⺻������ ��Ȱ��ȭ
    }

    void Update()
    {
        if (isDying) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float spawnDistance = Vector3.Distance(transform.position, spawnPoint);

        // �÷��̾� ���� �� ���� ����
        if (distance <= status.detectionSize.x && !isChasing)
        {
            isChasing = true;
            attackBox.SetActive(true); // �÷��̾� ���� �� AttackBox Ȱ��ȭ
            Debug.Log("[����] ���Ͱ� �÷��̾ �����ϰ� ������ �����մϴ�.");
        }

        // ���� ��, ���� �������� ��� ����
        if (isChasing)
        {
            if (distance <= status.chaseSize.x) // ���� ���� ��
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else // ���� ������ ����� Spawn Point�� ����
            {
                isChasing = false;
                attackBox.SetActive(false); // ���� ���� �� AttackBox ��Ȱ��ȭ
                agent.isStopped = false;
                agent.SetDestination(spawnPoint);
                Debug.Log("[�̵�] ���Ͱ� ��������Ʈ�� ���� ��...");
            }
        }

        // Spawn Point�� �̵� ��, �����ϸ� ��� ���·� ����
        if (!isChasing && spawnDistance < 0.5f)
        {
            agent.isStopped = true;
            Debug.Log("[���] ���Ͱ� Spawn Point�� �����Ͽ� ��� ���·� ��ȯ.");
        }

        /*
        // ü�� UI ������Ʈ
        if (hpBar != null)
        {
            nowHpBar.fillAmount = (float)status.hp / baseStatus.hp;
            hpBar.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        }
        */

        // ü���� 0 �����̸� ���
        if (status.hp <= 0 && !isDying)
        {
            Die();
        }
    }

    public void StopMovement()
    {
        agent.isStopped = true; // ���� �� ���߰� �ϱ�
    }

    public void ResumeMovement()
    {
        agent.isStopped = false; // ���� �� �ٽ� �̵�
    }

    void Die()
    {
        isDying = true;
        agent.isStopped = true;
        attackBox.SetActive(false); // ��� �� AttackBox ��Ȱ��ȭ
        Debug.Log("[���] ���Ͱ� ����߽��ϴ�.");

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
