/*******************************************************
 * EnemyAI.cs  – null 비교로 정리한 최종본
 *******************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IDamageable
{
    /* ───── Inspector ───── */
    public EnemyStatus status;
    [SerializeField] private SpawnArea spawnArea;
    public GameObject damageTextPrefab;

    /* ───── Runtime ───── */
    public NavMeshAgent agent { get; private set; }
    public Animator animator;
    private SpriteRenderer sr;

    private DebuffController debuff;
    public DebuffController debuffController { get => debuff; private set => debuff = value; }

    private MonsterTargeting targeting;
    private IMonsterAttack attackStrategy;
    private BehaviorTreeRunner bt;

    private Transform targetPlayer;
    private float lastMoveX = 1f;
    private string currentAnim = "";

    private bool canAttack = true; float coolT;
    private bool atkAnim; float atkT;
    private bool prepping; float prepT;
    private float atkDur = .7f;

    private bool waiting; float waitT, waitDur;
    private Vector3 wanderTarget;

    private float maxHP, hp; bool dead;
    private const float DIE_DUR = 1.5f;

    public static int ActiveMonsterCount;
    private UIEnemyHealthBar uiBar;

    /* ───────── Awake ───────── */
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        debuff = GetComponent<DebuffController>();
        targeting = GetComponent<MonsterTargeting>();
        attackStrategy = GetComponent<IMonsterAttack>();

        agent.updateRotation = false;
        agent.stoppingDistance = .1f;

        maxHP = hp = status.hp;
        atkDur = status.animDuration;
        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount++;

        EnsureSpawnArea();

        var bar = Resources.Load<GameObject>("UIEnemyHealthBar");
        if (bar != null)
        {
            var go = Instantiate(bar);
            uiBar = go.GetComponent<UIEnemyHealthBar>();
            uiBar.Init(transform, Vector3.up * status.headOffset);
            uiBar.SetHP(1f);
        }

        bt = new BehaviorTreeRunner(BuildBT());

        if (PhotonNetwork.InRoom && (!photonView.IsMine || !PhotonNetwork.IsMasterClient))
            agent.enabled = false;
    }

    /* ───────── Start ───────── */
    void Start() => StartCoroutine(DebugSpawnState());
    IEnumerator DebugSpawnState()
    {
        yield return null;
        Debug.Log($"[SPAWN-DBG] {name}  " +
                  $"Radius={(spawnArea != null ? spawnArea.GetRadius().ToString("F1") : "NULL")}  " +
                  $"OnNav={agent.isOnNavMesh} Path={agent.hasPath}");
        if (agent.enabled && agent.isOnNavMesh && EnsureSpawnArea())
            PickWanderPoint();
    }

    void OnTransformParentChanged()
    {
        if (EnsureSpawnArea())
        {
            if (agent.enabled && agent.isOnNavMesh) PickWanderPoint();
            Debug.Log($"[PARENT] {name} linked. radius={spawnArea.GetRadius()}");
        }
    }
    public void SetSpawnArea(SpawnArea sa)
    {
        spawnArea = sa;
        if (agent.enabled && agent.isOnNavMesh) PickWanderPoint();
    }

    /* ───────── Update ───────── */
    void Update()
    {
        if (!agent.enabled || dead) return;

        if (!canAttack)
        {
            coolT += Time.deltaTime;
            if (coolT >= status.attackCool) { canAttack = true; coolT = 0f; }
        }

        bt.Operate();

        if (atkAnim)
        {
            atkT += Time.deltaTime;
            if (atkT >= atkDur)
            {
                atkAnim = false;
                PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
            }
            return;
        }

        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f)) lastMoveX = vx;

        if (!agent.isStopped)
        {
            if (vx > 0) PlayAnim("Run_Right");
            else if (vx < 0) PlayAnim("Run_Left");
            else PlayAnim(lastMoveX >= 0 ? "Run_Right" : "Run_Left");
        }
        else PlayAnim(lastMoveX >= 0 ? "Right_Idle" : "Left_Idle");
    }

    /* ───── Behavior Tree ───── */
    INode BuildBT() => new SelectorNode(new List<INode>()
    {
        new SequenceNode(new List<INode>()
        {
            new ActionNode(CheckEnemyInRange),
            new ActionNode(DoAttack),
        }),
        new SequenceNode(new List<INode>()
        {
            new ActionNode(DetectEnemy),
            new ActionNode(MoveToEnemy),
        }),
        new ActionNode(WanderInsideArea),
    });

    INode.NodeState CheckEnemyInRange()
    {
        if (!canAttack || targetPlayer == null) return INode.NodeState.Failure;
        return Vector3.Distance(transform.position, targetPlayer.position) < status.attackRange
               ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState DetectEnemy()
    {
        Transform near = null; float min = status.detectRange;
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < min) { min = d; near = p.transform; }
        }
        targetPlayer = near;
        return near != null ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState MoveToEnemy()
    {
        if (targetPlayer == null) return INode.NodeState.Failure;

        if (targeting != null && targeting.monsterType == MonsterType.Ranged &&
            Vector3.Distance(transform.position, targetPlayer.position) <= status.attackRange)
        {
            agent.isStopped = true; agent.ResetPath();
            return INode.NodeState.Failure;
        }

        agent.isStopped = false;
        agent.speed = status.chaseSpeed;
        agent.SetDestination(new Vector3(targetPlayer.position.x,
                                         transform.position.y,
                                         targetPlayer.position.z));
        return INode.NodeState.Running;
    }

    INode.NodeState DoAttack()
    {
        if (!canAttack || attackStrategy == null || targetPlayer == null)
            return INode.NodeState.Failure;

        if (prepping)
        {
            prepT += Time.deltaTime;
            if (prepT >= status.waitCool)
            {
                prepping = false; prepT = 0f;
                string a = targetPlayer.position.x >= transform.position.x ? "Attack_Right" : "Attack_Left";
                PlayAnim(a);

                attackStrategy.Attack(targetPlayer);

                atkAnim = true; atkT = 0f; canAttack = false; coolT = 0f;
                return INode.NodeState.Success;
            }
            PlayAnim(targetPlayer.position.x >= transform.position.x ? "Right_Idle" : "Left_Idle");
            return INode.NodeState.Running;
        }
        else { prepping = true; prepT = 0f; agent.isStopped = true; return INode.NodeState.Running; }
    }

    INode.NodeState WanderInsideArea()
    {
        if (!EnsureSpawnArea()) return INode.NodeState.Failure;

        if (waiting)
        {
            if ((waitT += Time.deltaTime) >= waitDur)
            { waiting = false; agent.isStopped = false; PickWanderPoint(); }
            return INode.NodeState.Running;
        }

        if (agent.pathPending) return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + .1f)
        {
            waiting = true; agent.isStopped = true;
            waitDur = Random.Range(3f, 5f); waitT = 0f;
        }
        return INode.NodeState.Running;
    }

    void PickWanderPoint()
    {
        if (!EnsureSpawnArea()) return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 p = spawnArea.GetRandomPointInsideArea();
            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                agent.speed = status.wanderSpeed;
                agent.SetDestination(wanderTarget);
                return;
            }
        }
        Debug.LogWarning($"[WANDER] {name} failed (radius={spawnArea.GetRadius()})");
    }

    /* ───── Damage / Death ───── */
    public void TakeDamage(float dmg)
        => photonView.RPC(nameof(DamageToMaster), RpcTarget.MasterClient, dmg);

    [PunRPC]
    public void DamageToMaster(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient || dead) return;
        hp = Mathf.Max(0, hp - dmg);
        photonView.RPC(nameof(UpdateHP), RpcTarget.AllBuffered, hp);
        if (hp <= 0) Die();
    }
    [PunRPC] public void UpdateHP(float h) { hp = h; uiBar?.SetHP(hp / maxHP); }

    void Die()
    {
        if (dead) return; dead = true; agent.isStopped = true;
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All,
                       lastMoveX >= 0 ? "Right_Death" : "Left_Death");
        if (PhotonNetwork.IsMasterClient) StartCoroutine(CoDestroyLater());
        if (PhotonNetwork.IsMasterClient) ActiveMonsterCount--;
    }
    IEnumerator CoDestroyLater() { yield return new WaitForSeconds(DIE_DUR); if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject); }

    /* ───── RPC & Utils ───── */
    void PlayAnim(string a)
    {
        if (currentAnim == a) return;
        animator.Play(a); currentAnim = a;
        photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.Others, a);
    }
    [PunRPC] public void RPC_PlayAnim(string a) { animator.Play(a); currentAnim = a; }

    bool EnsureSpawnArea()
    {
        if (spawnArea != null) return true;
        spawnArea = GetComponentInParent<SpawnArea>();
        return spawnArea != null;
    }

    /* ───── Gizmos ───── */
    void OnDrawGizmos()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (agent.velocity.sqrMagnitude > .01f)
            {
                Gizmos.color = Color.blue;
                Vector3 dir = agent.velocity.normalized;
                Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
                Gizmos.DrawSphere(transform.position + dir * 1.5f, .07f);
            }
            if (agent.hasPath)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, agent.destination);
                Gizmos.DrawWireSphere(agent.destination, .25f);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, .25f);
        if (wanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(wanderTarget, .25f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}
