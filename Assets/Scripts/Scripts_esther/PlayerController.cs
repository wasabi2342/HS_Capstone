using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : BasePlayerController
{
    // 로컬 플레이어 정적 참조
    public static PlayerController localPlayer;

    [Header("이동 속도 설정 (8방향)")]
    public float moveSpeedHorizontal = 5f;
    public float moveSpeedVertical = 5f;
    public float moveSpeedDiagonalLeftUp = 4.5f;
    public float moveSpeedDiagonalRightUp = 4.5f;
    public float moveSpeedDiagonalLeftDown = 4.5f;
    public float moveSpeedDiagonalRightDown = 4.5f;

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

    // 공격 관련 Animator 파라미터
    private bool isAttacking = false;
    private int attackIndex = 0;
    private bool isDead = false;

    // 자식 전용 대쉬 플래그 
    private bool isDashingChild = false;

   
    private PlayerInputActions playerInputActions;

    public override void OnEnable()
    {
        base.OnEnable();
        if (playerInputActions == null)
            playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (playerInputActions != null)
            playerInputActions.Disable();
    }

    void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayerController] Animator 컴포넌트가 없습니다!");
        }

        if (photonView != null && photonView.IsMine)
        {
            localPlayer = this;
        }
        else
        {
            this.enabled = false;
            return;
        }

        basicAttackStack = 0;
        lastBasicAttackStackTime = Time.time;
        currentState = PlayerState.Idle; // 부모의 enum 사용
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

    // 부모의 OnMove override
    public override void OnMove(InputAction.CallbackContext context)
    {
        base.OnMove(context);
    }

    protected override void Update()
    {
        base.Update();

        if (!photonView.IsMine) return;
        if (isDead || currentState == PlayerState.Death) return;

        HandleActions();

        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            ResetBasicAttackStack();
        }

        HandleMovement();

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetBool("isDashing", isDashing); // 부모의 isDashing 사용
            animator.SetBool("isDead", isDead);
        }
    }

    #region 이동 & 대쉬

    protected override void HandleMovement()
    {
        bool canMove = (currentState != PlayerState.Attack_L &&
                        currentState != PlayerState.Attack_R &&
                        currentState != PlayerState.Skill &&
                        currentState != PlayerState.Ultimate &&
                        currentState != PlayerState.Death);

        if (!canMove) return;

        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        float h = inputVector.x;
        float v = inputVector.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            if (animator != null)
                animator.SetBool("isRunning", true);

            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            float moveSpeed = moveSpeedVertical;
            if (Mathf.Abs(h) > 0 && Mathf.Abs(v) < 0.01f)
                moveSpeed = moveSpeedHorizontal;
            else
            {
                if (v > 0)
                {
                    if (h > 0) moveSpeed = moveSpeedDiagonalRightUp;
                    else if (h < 0) moveSpeed = moveSpeedDiagonalLeftUp;
                }
                else if (v < 0)
                {
                    if (h > 0) moveSpeed = moveSpeedDiagonalRightDown;
                    else if (h < 0) moveSpeed = moveSpeedDiagonalLeftDown;
                }
            }

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
        if (playerInputActions.Player.Dash.triggered)
        {
            if (Time.time - lastDashClickTime <= dashDoubleClickThreshold)
            {
                Vector2 dashInput = playerInputActions.Player.Move.ReadValue<Vector2>();
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

    protected override IEnumerator DoDash(Vector3 direction)
    {
        isDashing = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * dashDistance;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        isDashing = false;
        yield return null;
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

    #region 공격 & 스킬 (변경 없음)

    private void HandleActions()
    {
        if (isAttacking) return;

        bool basicAttackInput = playerInputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = playerInputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = playerInputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = playerInputActions.Player.UltimateAttack.triggered;

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
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
            basicAttackStack = 1;
        else if (basicAttackStack < 4)
            basicAttackStack++;

        lastBasicAttackTime = Time.time;
        lastBasicAttackStackTime = Time.time;

        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);

        StartCoroutine(CoPerformAttack(1));
    }

    IEnumerator CoPerformAttack(int index)
    {
        isAttacking = true;
        attackIndex = index;

        switch (index)
        {
            case 1: currentState = PlayerState.Attack_L; break;
            case 2: currentState = PlayerState.Attack_R; break;
            case 3: currentState = PlayerState.Skill; break;
            case 4: currentState = PlayerState.Ultimate; break;
        }

        yield return new WaitForSeconds(0.2f);

        float range = (index == 1) ? basicAttackRange :
                      (index == 2) ? specialAttackRange :
                      (index == 3) ? skillRange : ultimateRange;
        int damage = (index == 1) ? basicAttackDamage :
                     (index == 2) ? specialAttackDamage :
                     (index == 3) ? skillDamage : ultimateDamage;

        AttackOverlapCheck(range, damage);

        if (basicAttackStack >= 4)
        {
            yield return new WaitForSeconds(0.3f);
            ResetBasicAttackStack();
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        Vector2 moveVal = playerInputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveVal.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;

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

    #region 상호작용 (Invoke Unity Events)

   
    public void OnTrapClear(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Trap"))
            {
                currentTrap = col.gameObject;
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
    }

    
    public void OnNPCInteract(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                Debug.Log($"[PlayerController] NPC와 상호작용! : {col.name}");
                
            }
        }
    }

    #endregion

    #region 데미지 & 사망

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
        if (isDead) return;
        currentState = PlayerState.Death;
        isDead = true;
        Debug.Log("[PlayerController] 플레이어 사망!");

        isAttacking = false;
        attackIndex = 0;
        isDashing = false;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }

    #endregion
}
