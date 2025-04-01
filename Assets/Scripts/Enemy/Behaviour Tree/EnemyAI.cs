using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviourPun, IDamageable
{
    public EnemyStatus status;  // EnemyStatus에 공격 데미지, 속도, 범위, 체력 등이 정의되어 있음
    public static int ActiveMonsterCount = 0;

    [SerializeField] private SpawnArea spawnArea;
    public GameObject damageTextPrefab;
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    private Transform targetPlayer;
    private NavMeshAgent agent;
    private BehaviorTreeRunner behaviorTree;
    private IMonsterAttack attackStrategy;

    private bool isWaiting = false;
    private float waitTime = 0f;
    private float waitTimer = 0f;
    private Vector3 wanderTarget = Vector3.zero;

    
    private float lastMoveX = 1f;
    private string currentMoveAnim = "";
    private bool canAttack = true;
    private float cooldownTimer = 0f;
    private bool isAttackAnimationPlaying = false;
    private float attackAnimTime = 0f;
    private float attackAnimDuration = 0.7f; // 기본값, status.animDuration으로 덮어씀
    private string prevAnimBeforeAttack = "";

    // 체력 관련 변수
    private float currentHP;
    // 사망 플래그 및 애니메이션 지속 시간
    private bool isDead = false;
    private float deathAnimDuration = 1.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        agent.updateRotation = false;
        agent.angularSpeed = 500f;
        agent.stoppingDistance = 0.1f;
        ActiveMonsterCount++;
        Debug.Log("EnemyAI Awake: " + gameObject.name + " ActiveMonsterCount: " + ActiveMonsterCount);

        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            Debug.Log($"[INIT] spawnArea auto-assigned: {spawnArea}");
        }

        attackStrategy = GetComponent<IMonsterAttack>();
        attackAnimDuration = status.animDuration;
        behaviorTree = new BehaviorTreeRunner(SettingBT());

        // EnemyStatus에 정의된 체력을 사용하여 초기화
        currentHP = status.hp;
    }

    private void Update()
    {
        // Master Client에서만 AI 로직 실행 및 사망 시 업데이트 중지
        if (!PhotonNetwork.IsMasterClient || isDead)
            return;

        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= status.attackCool)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }

        behaviorTree.Operate();

        if (isAttackAnimationPlaying)
        {
            attackAnimTime += Time.deltaTime;
            if (attackAnimTime >= attackAnimDuration)
            {
                isAttackAnimationPlaying = false;
                attackAnimTime = 0f;
                if (!string.IsNullOrEmpty(prevAnimBeforeAttack))
                    PlayMoveAnim(prevAnimBeforeAttack);
            }
            return;
        }

        float vx = agent.velocity.x;
        if (!Mathf.Approximately(vx, 0f))
        {
            lastMoveX = vx;
        }

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

    // 애니메이션 재생 및 RPC 동기화
    void PlayMoveAnim(string animName)
    {
        if (currentMoveAnim == animName)
            return;

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

    INode SettingBT()
    {
        return new SelectorNode(
            new List<INode>()
            {
                new SequenceNode(
                    new List<INode>()
                    {
                        new ActionNode(CheckEnemyWithinAttackRange),
                        new ActionNode(DoAttack),
                    }
                ),
                new SequenceNode(
                    new List<INode>()
                    {
                        new ActionNode(CheckDetectEnemy),
                        new ActionNode(MoveToEnemy),
                    }
                ),
                new ActionNode(WanderInsideSpawnArea),
            }
        );
    }

    INode.NodeState CheckEnemyWithinAttackRange()
    {
        if (!canAttack)
            return INode.NodeState.Failure;

        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist < status.attackRange)
                return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    INode.NodeState DoAttack()
    {
        if (targetPlayer != null && attackStrategy != null && canAttack)
        {
            prevAnimBeforeAttack = currentMoveAnim;
            float targetX = targetPlayer.position.x;
            float selfX = transform.position.x;
            string attackAnim = targetX >= selfX ? "Attack_Right" : "Attack_Left";
            PlayMoveAnim(attackAnim);

            // 공격 실행 (Master Client 기준)
            attackStrategy.Attack(targetPlayer);

            isAttackAnimationPlaying = true;
            attackAnimTime = 0f;
            canAttack = false;
            cooldownTimer = 0f;

            return INode.NodeState.Success;
        }
        return INode.NodeState.Failure;
    }

    INode.NodeState CheckDetectEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = status.detectRange;
        Transform nearestPlayer = null;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestPlayer = player.transform;
            }
        }
        targetPlayer = nearestPlayer;
        return targetPlayer != null ? INode.NodeState.Success : INode.NodeState.Failure;
    }

    INode.NodeState MoveToEnemy()
    {
        if (targetPlayer != null)
        {
            Vector3 targetPos = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
            agent.speed = status.chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            return INode.NodeState.Running;
        }
        return INode.NodeState.Failure;
    }

    INode.NodeState WanderInsideSpawnArea()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                waitTime = 0f;
                agent.isStopped = false;
                ChooseNextWanderDestination();
            }
            return INode.NodeState.Running;
        }

        if (agent.pathPending)
            return INode.NodeState.Running;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            isWaiting = true;
            agent.isStopped = true;
            waitTime = Random.Range(3f, 5f);
            waitTimer = 0f;
            Debug.Log($"[Wander] 목적지 도착. {waitTime:F2}초 대기 시작.");
        }
        return INode.NodeState.Running;
    }

    private void ChooseNextWanderDestination()
    {
        if (spawnArea == null)
        {
            spawnArea = GetComponentInParent<SpawnArea>();
            Debug.LogWarning("[BT] WanderInsideSpawnArea: spawnArea was null, attempted reassignment.");
        }
        if (spawnArea == null || agent == null)
        {
            Debug.LogWarning("[BT] WanderInsideSpawnArea: Missing spawnArea or agent");
            return;
        }

        const int maxTries = 10;
        const float minDistToOthers = 2.5f;
        const float minWanderDistance = 1.5f;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 candidate = spawnArea.GetRandomPointInsideArea();
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, hit.position);
                if (dist < minWanderDistance)
                    continue;

                if (!IsNearOtherMonsters(hit.position, minDistToOthers))
                {
                    wanderTarget = hit.position;
                    wanderTarget.y = transform.position.y;
                    if (agent.SetDestination(wanderTarget))
                    {
                        agent.speed = status.wanderSpeed;
                        Debug.Log($"[Wander] 새 목적지: {wanderTarget} (dist: {dist:F2}m)");
                        return;
                    }
                }
            }
        }
        Debug.LogWarning("[Wander] Failed to find valid destination after retries");
    }

    private bool IsNearOtherMonsters(Vector3 pos, float minDist)
    {
        foreach (var other in FindObjectsOfType<EnemyAI>())
        {
            if (other != this && Vector3.Distance(other.transform.position, pos) < minDist)
                return true;
        }
        return false;
    }

    public void OnAttackHitEvent()
    {
        if (targetPlayer != null && attackStrategy != null)
        {
            attackStrategy.Attack(targetPlayer);
        }
    }

    // ---------------------------
    // IDamageable 인터페이스 구현
    // ---------------------------

    // 플레이어나 다른 오브젝트가 이 몬스터에게 데미지를 입힐 때 호출됩니다.
    // 이 메서드는 RPC를 통해 Master Client에서 DamageToMaster를 호출합니다.
    public void TakeDamage(float damage)
    {
        photonView.RPC("DamageToMaster", RpcTarget.MasterClient, damage);
    }

    // Master Client에서 데미지 계산을 수행하는 RPC 메서드입니다.
    [PunRPC]
    public void DamageToMaster(float damage)
    {
        if (!PhotonNetwork.IsMasterClient || isDead)
            return;

        string hitAnim = lastMoveX >= 0 ? "Right_Hit" : "Left_Hit";
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, hitAnim);


        currentHP -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, current HP: {currentHP}");
        photonView.RPC("UpdateHP", RpcTarget.AllBuffered, currentHP);

        photonView.RPC("RPC_FlashSprite", RpcTarget.All);
        SpawnDamageText(damage);
        

        if (currentHP <= 0)
        {
            Die();
        }
    }
    [PunRPC]
    public void RPC_FlashSprite()
    {
        StartCoroutine(FlashSpriteCoroutine());
    }

    private IEnumerator FlashSpriteCoroutine()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Color flashColor;
            // #RRGGBBAA 형식으로 알파값(투명도)까지 지정
            if (!ColorUtility.TryParseHtmlString("#FF000080", out flashColor))
            {
                // 변환 실패 시 기본 빨간색
                flashColor = Color.red;
            }

            // 스프라이트 색상 변경
            spriteRenderer.color = flashColor;
            Debug.Log($"[FlashSprite] {gameObject.name} 몬스터 색상을 {flashColor}로 변경했습니다.");

            // 잠시 대기
            yield return new WaitForSeconds(0.1f);

            // 원래 색상 복원
            spriteRenderer.color = originalColor;
        }
    }



    private void SpawnDamageText(float damage)
    {
        // 1) Resources 폴더에서 DamageText 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>("DamageText");
        if (prefab == null)
        {
            Debug.LogWarning("DamageText 프리팹을 찾지 못했습니다. (Resources 폴더 확인)");
            return;
        }

        // 2) 몬스터 머리 위에 배치할 위치 계산
        Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);

        // 3) 프리팹 인스턴스화
        GameObject dmgText = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 4) TextMesh에 데미지 수치 설정 (DamageTextBehaviour.cs 참고)
        DamageText text = dmgText.GetComponent<DamageText>();
        if (text != null)
        {
            text.SetDamage(damage);
        }
        else
        {
            // TextMesh가 직결되어 있다면, 아래 직접 접근
            TextMesh textMesh = dmgText.GetComponent<TextMesh>();
            if (textMesh != null)
                textMesh.text = damage.ToString();
        }
    }


    // 모든 클라이언트에서 체력 상태를 업데이트합니다.
    [PunRPC]
    public void UpdateHP(float hp)
    {
        currentHP = hp;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Master Client에서만 카운터를 감소시키도록 함 (중복 감소 방지)
        if (PhotonNetwork.IsMasterClient)
        {
            ActiveMonsterCount = Mathf.Max(ActiveMonsterCount - 1, 0);
            Debug.Log($"{gameObject.name} died. Updated ActiveMonsterCount: {ActiveMonsterCount}");
        }

        // 사망 애니메이션 실행
        string deathAnim = lastMoveX >= 0 ? "Right_Death" : "Left_Death";
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, deathAnim);

        // Death 애니메이션이 끝난 후 Destroy를 호출하기 위해 코루틴 실행
        StartCoroutine(DelayedDestroy());
    }


    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (wanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(wanderTarget, 0.4f);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}
