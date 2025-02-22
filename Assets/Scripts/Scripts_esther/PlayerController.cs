using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks
{
    // 로컬 플레이어 정적 참조
    public static PlayerController localPlayer;

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

    // Animator 파라미터
    private bool isDashing = false;   // 대쉬 중인지
    private int attackIndex = 0;      // 1=Attack_L, 2=Attack_R, 3=Skill, 4=Ultimate
    private bool isDead = false;      // 사망 여부

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
        if (photonView != null && photonView.IsMine)
        {
            localPlayer = this;
        }
        else
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
            animator.SetBool("isAttacking", false);
            animator.SetInteger("AttackIndex", 0);
            animator.SetBool("isDashing", false);
            animator.SetBool("isDead", false);
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
        // StageProgress (G키)
        if (inputActions.Player.StageProgress.triggered)
        {
            Debug.Log("[PlayerController] 다음 스테이지로 넘어갈 예정입니다!");
        }

        // UI / 대화 중이면 입력 무시
        if (UIManager_player.Instance != null)
        {
            if (UIManager_player.Instance.pauseMenuPanel != null && UIManager_player.Instance.pauseMenuPanel.activeSelf)
                return;

            if (UIManager_player.Instance.IsDialogueActive())
            {
                if (inputActions.Player.NPCInteract.triggered)
                {
                    UIManager_player.Instance.NextDialogue();
                }
                return;
            }
        }

        // 사망 상태면 모든 입력 무시
        if (isDead || currentState == PlayerState.Death)
            return;

        // 일시정지
        if (inputActions.Player.Pause.triggered)
        {
            UIManager_player.Instance?.TogglePauseMenu();
            return;
        }

        // 대쉬(더블클릭)
        HandleDash();

        // 평타 스택 리셋
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            ResetBasicAttackStack();
        }

        // 공격/스킬 입력
        HandleActions();

        // 이동 처리
        HandleMovement();

        // 중심점 업데이트
        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        // 상호작용
        if (!isAttacking && currentState != PlayerState.Skill && currentState != PlayerState.Ultimate)
        {
            CheckInteractions();
        }

        // --- Animator 파라미터를 매 프레임 갱신
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetBool("isDashing", isDashing);
            animator.SetBool("isDead", isDead);
        }
    }

    #region 이동 & 대쉬

    private void HandleMovement()
    {
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
            if (Mathf.Abs(h) > 0 && Mathf.Abs(v) < 0.01f)
                moveSpeed = speedHorizontal;

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

            if (animator != null)
            {
                animator.SetFloat("moveX", moveDir.x);
                animator.SetFloat("moveY", moveDir.z);
            }
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
                Vector2 dashInput = inputActions.Player.Move.ReadValue<Vector2>();
                Vector3 dashDir = new Vector3(dashInput.x, 0, dashInput.y);
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
        isDashing = true;

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

        // 대쉬 종료
        isDashing = false;

        // Idle or Run
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
        // 스택 리셋 시간 지났으면 1부터 시작, 아니면 스택++
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
            basicAttackStack = 1;
        else if (basicAttackStack < 4)
            basicAttackStack++;

        lastBasicAttackTime = Time.time;
        lastBasicAttackStackTime = Time.time;

        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);

        // Attack_L = 1
        StartCoroutine(CoPerformAttack(1));
    }

    IEnumerator CoPerformAttack(int index)
    {
        isAttacking = true;
        attackIndex = index; // 1=Attack_L, 2=Attack_R, 3=Skill, 4=Ultimate

        // 상태 설정
        switch (index)
        {
            case 1: currentState = PlayerState.Attack_L; break;
            case 2: currentState = PlayerState.Attack_R; break;
            case 3: currentState = PlayerState.Skill; break;
            case 4: currentState = PlayerState.Ultimate; break;
        }

        // 공격 모션 시작 후 약간 대기(타격 타이밍)
        yield return new WaitForSeconds(0.2f);

        // 공격 판정
        float range = (index == 1) ? basicAttackRange :
                      (index == 2) ? specialAttackRange :
                      (index == 3) ? skillRange : ultimateRange;
        int damage = (index == 1) ? basicAttackDamage :
                     (index == 2) ? specialAttackDamage :
                     (index == 3) ? skillDamage : ultimateDamage;

        AttackOverlapCheck(range, damage);

        // ★ 만약 AttackStack이 4까지 찼다면 (콤보의 마지막)
        //    추가 입력 없이도 자동으로 Idle로 돌아가게 처리
        if (basicAttackStack >= 4)
        {
            // 마지막 평타 모션 재생 후 대기
            yield return new WaitForSeconds(0.3f);

            // 스택 리셋
            ResetBasicAttackStack();
            // Idle or Run
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        }
        else if (index == 1 && basicAttackStack >= 4)
        {
            // 위와 동일 로직이나, "AttackStack=4"를 처리할 때
            // (현재 index=1일 때 4연타가 완성되면 여기서도 처리 가능)
            // -- 이 로직은 상황에 따라 조정
        }
        else
        {
            // AttackStack이 4 미만일 때
            yield return new WaitForSeconds(0.3f);

            // 이동 입력 체크
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        }

        // 공격 종료
        isAttacking = false;
        attackIndex = 0;
    }

    IEnumerator PerformSpecialAttack()
    {
        lastSpecialAttackTime = Time.time;
        yield return StartCoroutine(CoPerformAttack(2));
    }

    IEnumerator PerformSkillAttack()
    {
        lastSkillTime = Time.time;
        yield return StartCoroutine(CoPerformAttack(3));
    }

    IEnumerator PerformUltimateAttack()
    {
        lastUltimateTime = Time.time;
        yield return StartCoroutine(CoPerformAttack(4));
    }

    #endregion

    #region 공격 판정 & 평타 스택

    private void AttackOverlapCheck(float range, int damage)
    {
        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, range, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
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

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return; // 중복 호출 방지

        currentState = PlayerState.Death;
        isDead = true; // Animator 파라미터도 동기화

        Debug.Log("[PlayerController] 플레이어 사망!");

        // 사망 시 공격/대쉬 상태 해제
        isAttacking = false;
        attackIndex = 0;
        isDashing = false;

        // Animator에 Death 파라미터 전달 (Bool)
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }

    #endregion
}
