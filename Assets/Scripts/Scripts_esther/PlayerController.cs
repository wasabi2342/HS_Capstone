using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : BasePlayerController
{
    // 로컬 플레이어 정적 참조
    public static PlayerController localPlayer;

    [Header("8방향 이동 속도 설정")]
    public float moveSpeedHorizontal = 5f;
    public float moveSpeedVertical = 5f;
    public float moveSpeedDiagonalLeftUp = 4.5f;
    public float moveSpeedDiagonalRightUp = 4.5f;
    public float moveSpeedDiagonalLeftDown = 4.5f;
    public float moveSpeedDiagonalRightDown = 4.5f;

    // centerPoints
    [Header("중심점 설정 (8개)")]
    [SerializeField] private Transform[] centerPoints = new Transform[8];

    // 입력 액션
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

    protected override void Start()
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
    }

    // 부모의 OnMove 사용
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
        HandleMovement();

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            animator.SetInteger("AttackIndex", attackIndex);
            animator.SetBool("isDashing", isDashing);
            animator.SetBool("isDead", isDead);
        }
    }

    #region 8방향 이동

    protected override void HandleMovement()
    {
        if (currentState == PlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
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
    // 공격/스킬 입력 및 로직은 부모로부터 호출
    protected override void HandleActions()
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
            PerformSpecialAttack();
        }
        else if (skillAttackInput && Time.time - lastSkillTime >= skillCooldown)
        {
            PerformSkillAttack();
        }
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= ultimateCooldown)
        {
            PerformUltimateAttack();
        }
    }

    protected override void PerformBasicAttack()
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

    protected override IEnumerator CoPerformAttack(int index)
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

        if (moveInput.magnitude > 0.1f)
            currentState = PlayerState.Run;
        else
            currentState = PlayerState.Idle;

        isAttacking = false;
        attackIndex = 0;
    }

    protected override void PerformSpecialAttack()
    {
        lastSpecialAttackTime = Time.time;
        StartCoroutine(CoPerformAttack(2));
    }

    protected override void PerformSkillAttack()
    {
        lastSkillTime = Time.time;
        StartCoroutine(CoPerformAttack(3));
    }

    protected override void PerformUltimateAttack()
    {
        lastUltimateTime = Time.time;
        StartCoroutine(CoPerformAttack(4));
    }

    protected override void AttackOverlapCheck(float range, int damage)
    {
        if (centerPoint == null) return;
        Vector3 checkPos = centerPoint.position;
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
                Debug.Log($"[PlayerController] 공격: {damage} 데미지 → 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
    }

    protected override void ResetBasicAttackStack()
    {
        basicAttackStack = 0;
        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);
    }
    #endregion

    #region 상호작용 (Invoke Unity Events)

    public override void OnNPCInteract(InputAction.CallbackContext context)
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

    public override void OnTrapClear(InputAction.CallbackContext context)
    {
        base.OnTrapClear(context);
    }
    #endregion

    #region 데미지 & 사망 (공통은 부모 사용)
    
    #endregion
}
