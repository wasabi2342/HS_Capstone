using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class ServantAI : MonoBehaviourPun
{
    [Header("참조")]
    public Animator animator;
    public Transform masterPlayer;
    [Header("설정")]
    public const float FollowRadius = 10f;      
    public float followRadius = 6f;
    public float detectRange = 8f;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;

    private NavMeshAgent agent;
    private IMonsterAttack attackStrategy;
    private Transform targetEnemy;
    private BehaviorTreeRunner behaviorTree;

    private float cooldownTimer = 0f;
    private bool canAttack = true;
    private float lastMoveX = 1f;
    private string currentMoveAnim = "";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        attackStrategy = GetComponent<IMonsterAttack>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0.1f;
        behaviorTree = new BehaviorTreeRunner(SettingBT());
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || masterPlayer == null)
            return;

        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        behaviorTree.Operate();
        UpdateAnimation();
    }

    private INode SettingBT()
    {
        return new SelectorNode(new List<INode>()
        {
            new SequenceNode(new List<INode>()
            {
                new ActionNode(CheckEnemyInAttackRange),
                new ActionNode(DoAttack),
            }),
            new SequenceNode(new List<INode>()
            {
                new ActionNode(FindNearestEnemy),
                new ActionNode(ChaseEnemy),
            }),
            new ActionNode(WanderAroundMaster)
        });
    }

    private INode.NodeState CheckEnemyInAttackRange()
    {
        if (!canAttack || targetEnemy == null)
            return INode.NodeState.Failure;

        float dist = Vector3.Distance(transform.position, targetEnemy.position);
        return dist <= attackRange ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    private INode.NodeState DoAttack()
    {
        if (attackStrategy != null && targetEnemy != null)
        {
            attackStrategy.Attack(targetEnemy);
            PlayMoveAnim(targetEnemy.position.x >= transform.position.x ? "Attack_Right" : "Attack_Left");
            canAttack = false;
            if (PhotonNetwork.IsMasterClient)   // 로그 한 번만 찍도록
            {
                Debug.Log($"[ServantAI] {name} attacked <color=yellow>{targetEnemy.name}</color> at {Time.time:F2}s");
            }
            return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    private INode.NodeState FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDist = detectRange;
        Transform nearest = null;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= detectRange)
            {
                minDist = dist;
                nearest = enemy.transform;
            }
        }

        targetEnemy = nearest;
        return targetEnemy != null ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    private INode.NodeState ChaseEnemy()
    {
        if (targetEnemy == null)
            return INode.NodeState.Failure;

        agent.speed = chaseSpeed;
        agent.SetDestination(targetEnemy.position);
        return INode.NodeState.Running;
    }
    private void OnDrawGizmos()
    {
        if (masterPlayer == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(masterPlayer.position, FollowRadius);
    }
    private INode.NodeState WanderAroundMaster()
    {
        if (masterPlayer == null) return INode.NodeState.Failure;

        // 목적지가 없거나 거의 도착했으면 새 지점 뽑기
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            // 최대 10회 시도해 유효한 지점 찾기
            for (int i = 0; i < 10; i++)
            {
                Vector3 candidate =
                    masterPlayer.position + Random.insideUnitSphere * FollowRadius;
                candidate.y = transform.position.y;

                // NavMesh 위 + 반경 10m 안쪽인지 확인
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.5f, NavMesh.AllAreas) &&
                    Vector3.Distance(masterPlayer.position, hit.position) <= FollowRadius)
                {
                    agent.speed = wanderSpeed;
                    agent.SetDestination(hit.position);
                    break;
                }
            }
        }
        return INode.NodeState.Running;
    }

    private void UpdateAnimation()
    {
        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f))
            lastMoveX = vx;

        if (!agent.isStopped)
        {
            if (vx > 0f) PlayMoveAnim("Run_Right");
            else if (vx < 0f) PlayMoveAnim("Run_Left");
            else PlayMoveAnim(lastMoveX >= 0f ? "Run_Right" : "Run_Left");
        }
        else
        {
            PlayMoveAnim(lastMoveX >= 0f ? "Right_Idle" : "Left_Idle");
        }
    }

    private void PlayMoveAnim(string animName)
    {
        if (currentMoveAnim == animName) return;

        animator.Play(animName);
        currentMoveAnim = animName;
        photonView.RPC("RPC_PlayAnimation", RpcTarget.Others, animName);
    }

    [PunRPC]
    public void RPC_PlayAnimation(string animName)
    {
        animator.Play(animName);
        currentMoveAnim = animName;
    }

    [PunRPC]
    public void RPC_SetMaster(int masterViewID)
    {
        PhotonView masterPV = PhotonView.Find(masterViewID);
        if (masterPV != null)
            masterPlayer = masterPV.transform;
    }
}
