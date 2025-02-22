using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("이동 속도 설정 (8방향)")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;
    public float speedDiagonalLeftUp = 4.5f;
    public float speedDiagonalRightUp = 4.5f;
    public float speedDiagonalLeftDown = 4.5f;
    public float speedDiagonalRightDown = 4.5f;

    [Header("대쉬 설정")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    private float lastDashClickTime = -Mathf.Infinity;

    [Header("공격 및 스킬 데미지 설정")]
    public int basicAttackDamage = 10;
    public int specialAttackDamage = 15;
    public int skillDamage = 30;
    public int ultimateDamage = 50;

    [Header("공격 범위 설정")]
    public float basicAttackRange = 1.5f;
    public float specialAttackRange = 1.5f;
    public float skillRange = 2f;
    public float ultimateRange = 3f;

    [Header("쿨타임 설정 (초)")]
    public float basicAttackCooldown = 0.5f;
    public float specialAttackCooldown = 1f;
    public float skillCooldown = 2f;
    public float ultimateCooldown = 3f;

    [Header("평타 스택 초기화 (초)")]
    public float basicAttackResetTime = 2f;

    [Header("중심점 설정 (기본)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("중심점 설정 (8개)")]
    [SerializeField] private Transform[] centerPoints = new Transform[8];

    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("플레이어 데미지 범위 설정")]
    public SphereCollider damageCollider;
    public float damageColliderRadius = 0.7f;

    [Header("상호작용 설정")]
    public LayerMask interactionLayerMask;
    public float interactionRadius = 1.5f;

    // 함정 관련 변수
    private int trapClearCount = 0;
    private GameObject currentTrap = null;
    private bool isTrapCleared = false;

    // 공격 쿨타임 체크용
    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    // 평타 스택
    private int basicAttackStack = 0;
    private float lastBasicAttackStackTime = 0f;

    // Animator
    private Animator animator;

    // 공격 중 여부
    private bool isAttacking = false;

    // 플레이어 상태
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    private PlayerState currentState = PlayerState.Idle;

    // 인풋 시스템
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayerController] Animator 컴포넌트가 없습니다!");
        }

        // Photon 소유권 체크
        if (!photonView.IsMine)
        {
            this.enabled = false;
            return;
        }

        // 초기화
        basicAttackStack = 0;
        lastBasicAttackStackTime = Time.time;
        currentState = PlayerState.Idle;
        currentHealth = maxHealth;

        if (animator != null)
        {
            animator.SetInteger("AttackStack", basicAttackStack);
            animator.SetBool("isRunning", false);
        }

        if (damageCollider != null)
        {
            damageCollider.radius = damageColliderRadius;
        }

        if (centerPoint != null)
        {
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }
    }

    void Update()
    {
        // 혹시 모를 UI 매니저에서 일시정지나 대화 중이면 입력 무시
        if (UIManager_player.Instance != null)
        {
            // 일시정지 메뉴 확인
            if (UIManager_player.Instance.pauseMenuPanel != null && UIManager_player.Instance.pauseMenuPanel.activeSelf)
                return;

            // 대화 중이면 NextDialogue만
            if (UIManager_player.Instance.IsDialogueActive())
            {
                if (inputActions.Player.NPCInteract.triggered)
                {
                    UIManager_player.Instance.NextDialogue();
                }
                return;
            }
        }

        // 죽은 상태면 입력 무시
        if (currentState == PlayerState.Death)
            return;

        // 일시정지 입력 처리
        if (inputActions.Player.Pause.triggered)
        {
            UIManager_player.Instance?.TogglePauseMenu();
            return;
        }

        // 대쉬 처리(더블클릭)
        HandleDash();

        // 평타 스택 리셋 체크
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            ResetBasicAttackStack();
        }

        // 공격/스킬 입력
        HandleActions();

        // 이동 처리 (공격 중이 아니면)
        HandleMovement();

        // 중심점 업데이트
        if (centerPoint != null)
        {
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }

        // 상호작용 처리 (공격 중이 아닐 때만)
        if (!isAttacking && currentState != PlayerState.Skill && currentState != PlayerState.Ultimate)
        {
            CheckInteractions();
        }
    }

    #region 이동 & 대쉬

    private void HandleMovement()
    {
        // 공격, 스킬, 궁극 중이면 이동 불가
        bool canMove = (currentState != PlayerState.Attack_L &&
                        currentState != PlayerState.Attack_R &&
                        currentState != PlayerState.Skill &&
                        currentState != PlayerState.Ultimate &&
                        currentState != PlayerState.Death);

        if (!canMove) return;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float h = moveInput.x;
        float v = moveInput.y;

        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);

        if (isMoving)
        {
            currentState = PlayerState.Run;
            if (animator != null)
                animator.SetBool("isRunning", true);

            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            float moveSpeed = speedVertical;

            // 8방향 세밀한 속도 분기 필요 시, 여기서 조건 처리
            // 예: 좌우만 움직일 때 speedHorizontal, 대각일 때 별도 처리 등
            // (아래는 간단 예시. 실제로는 조건을 더 세밀히 구분하세요.)
            if (Mathf.Abs(h) > 0 && Mathf.Abs(v) < 0.01f)
                moveSpeed = speedHorizontal;

            // 이동
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

            // 애니메이터 파라미터
            if (animator != null)
            {
                animator.SetFloat("moveX", moveDir.x);
                animator.SetFloat("moveY", moveDir.z);
            }

            // 8개 centerPoints 업데이트 (원하는 경우에만)
            UpdateCenterPoints(moveDir);
        }
        else
        {
            currentState = PlayerState.Idle;
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }

    private void HandleDash()
    {
        if (inputActions.Player.Dash.triggered)
        {
            if (Time.time - lastDashClickTime <= dashDoubleClickThreshold)
            {
                // 더블 클릭 간격 내라면 대쉬
                Vector2 dashInput = inputActions.Player.Move.ReadValue<Vector2>();
                Vector3 dashDir = new Vector3(dashInput.x, 0, dashInput.y);

                // 입력이 없으면 바라보는 방향으로
                if (dashDir == Vector3.zero)
                    dashDir = transform.forward;

                StartCoroutine(DoDash(dashDir));
                lastDashClickTime = -Mathf.Infinity;
                return;
            }
            else
            {
                lastDashClickTime = Time.time;
            }
        }
    }

    IEnumerator DoDash(Vector3 direction)
    {
        if (animator != null)
            animator.SetTrigger("Dash");

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * dashDistance;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        // 대쉬 후 상태 복귀
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput.magnitude > 0.1f)
        {
            currentState = PlayerState.Run;
            if (animator != null)
                animator.SetBool("isRunning", true);
        }
        else
        {
            currentState = PlayerState.Idle;
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }

    private void UpdateCenterPoints(Vector3 moveDir)
    {
        if (centerPoints == null || centerPoints.Length != 8)
            return;

        for (int i = 0; i < centerPoints.Length; i++)
        {
            if (centerPoints[i] == null) continue;

            float angle = i * 45f;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * moveDir * centerPointOffsetDistance;
            centerPoints[i].position = transform.position + offset;
        }
    }

    #endregion

    #region 공격 & 스킬

    private void HandleActions()
    {
        // 이미 공격 중이면(모션 도중이면) 새로운 공격 입력 무시
        if (isAttacking) return;

        bool basicAttackInput = inputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = inputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = inputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = inputActions.Player.UltimateAttack.triggered;

        if (basicAttackInput && Time.time - lastBasicAttackTime >= basicAttackCooldown)
        {
            PerformBasicAttack();
        }
        else if (specialAttackInput && Time.time - lastSpecialAttackTime >= specialAttackCooldown)
        {
            StartCoroutine(PerformSpecialAttack());
        }
        else if (skillAttackInput && Time.time - lastSkillTime >= skillCooldown)
        {
            StartCoroutine(PerformSkillAttack());
        }
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= ultimateCooldown)
        {
            StartCoroutine(PerformUltimateAttack());
        }
    }

    private void PerformBasicAttack()
    {
        // 평타 스택 갱신
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            basicAttackStack = 1;
        }
        else
        {
            if (basicAttackStack < 4)
                basicAttackStack++;
        }

        lastBasicAttackTime = Time.time;
        lastBasicAttackStackTime = Time.time;

        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);

        // 코루틴 시작
        StartCoroutine(CoPerformBasicAttack());
    }

    IEnumerator CoPerformBasicAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Attack_L;

        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            animator.SetTrigger("Attack_L");
        }

        // 공격 판정 시점까지 대기(애니메이션에 맞춰 조절)
        yield return new WaitForSeconds(0.2f);

        // 실제 데미지 판정
        AttackOverlapCheck(basicAttackRange, basicAttackDamage);

        // 만약 스택이 4회 도달했다면, 일정 시간 안에 추가 입력을 받으면 연속 공격
        if (basicAttackStack >= 4)
        {
            float waitTimer = 0f;
            while (waitTimer < basicAttackResetTime)
            {
                if (inputActions.Player.BasicAttack.triggered)
                {
                    // 연속 공격
                    lastBasicAttackTime = Time.time;
                    lastBasicAttackStackTime = Time.time;
                    if (animator != null)
                        animator.SetInteger("AttackStack", basicAttackStack);

                    // 재귀적으로 같은 코루틴 재실행
                    yield return StartCoroutine(CoPerformBasicAttack());
                    yield break;
                }
                waitTimer += Time.deltaTime;
                yield return null;
            }

            // 여기까지 오면 스택 초기화
            ResetBasicAttackStack();
            currentState = PlayerState.Idle;
        }
        else
        {
            // 연속공격(스택 < 4)은 다음 모션까지 약간의 대기
            yield return new WaitForSeconds(0.3f);

            // 대기 후 이동 입력 확인 → Idle 또는 Run
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        }

        isAttacking = false;
        if (animator != null)
            animator.SetBool("isAttacking", false);
    }

    IEnumerator PerformSpecialAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Attack_R;

        lastSpecialAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack_R");

        yield return new WaitForSeconds(0.2f);

        // 실제 데미지 판정
        AttackOverlapCheck(specialAttackRange, specialAttackDamage);

        yield return new WaitForSeconds(0.3f);

        // 이동 입력 확인 후 상태 복귀
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;

        isAttacking = false;
    }

    IEnumerator PerformSkillAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Skill;

        lastSkillTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Skill");

        yield return new WaitForSeconds(0.2f);

        // 실제 데미지 판정
        AttackOverlapCheck(skillRange, skillDamage);

        yield return new WaitForSeconds(0.3f);

        // 이동 입력 확인 후 상태 복귀
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;

        isAttacking = false;
    }

    IEnumerator PerformUltimateAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Ultimate;

        lastUltimateTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Ultimate");

        yield return new WaitForSeconds(0.3f);

        // 실제 데미지 판정
        AttackOverlapCheck(ultimateRange, ultimateDamage);

        yield return new WaitForSeconds(0.3f);

        // 이동 입력 확인 후 상태 복귀
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;

        isAttacking = false;
    }

    /// <summary>
    /// OverlapSphere로 적을 찾아 데미지를 입힌다.
    /// </summary>
    /// <param name="range">공격 범위</param>
    /// <param name="damage">데미지</param>
    private void AttackOverlapCheck(float range, int damage)
    {
        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, range, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            // 가장 가까운 적 찾기
            Collider closest = cols[0];
            float minDist = Vector3.Distance(checkPos, closest.transform.position);
            foreach (Collider col in cols)
            {
                float dist = Vector3.Distance(checkPos, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col;
                }
            }
            EnemyController enemy = closest.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[공격] {damage} 데미지 → 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
    }

    /// <summary>
    /// 평타 스택을 0으로 초기화하고 Animator에도 반영
    /// </summary>
    private void ResetBasicAttackStack()
    {
        basicAttackStack = 0;
        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);
    }

    #endregion

    #region 상호작용 & 데미지

    private void CheckInteractions()
    {
        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);

        foreach (Collider col in cols)
        {
            // 함정
            if (col.gameObject.layer == LayerMask.NameToLayer("Trap"))
            {
                currentTrap = col.gameObject;
                if (inputActions.Player.TrapClear.triggered)
                {
                    trapClearCount++;
                    Debug.Log("함정 키 입력 횟수: " + trapClearCount);
                    if (trapClearCount >= 2)
                    {
                        isTrapCleared = true;
                        Debug.Log("함정 해제됨.");
                        Destroy(currentTrap);
                        trapClearCount = 0;
                        currentTrap = null;
                    }
                }
            }
            // NPC
            else if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                if (inputActions.Player.NPCInteract.triggered)
                {
                    UIManager_player.Instance?.StartDialogue();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("플레이어 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        currentState = PlayerState.Death;
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        else
        {
            Debug.LogWarning("[PlayerController] Animator가 null이라 Death 애니메이션 실행 불가.");
        }
    }

    #endregion
}
