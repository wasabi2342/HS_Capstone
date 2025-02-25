using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public abstract class BasePlayerController : MonoBehaviourPunCallbacks
{
    [Header("기본 이동 속도")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("대쉬 설정")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    protected float lastDashClickTime = -Mathf.Infinity;

    [Header("중심점 설정")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("상호작용/데미지 범위 설정")]
    public LayerMask interactionLayerMask;   // NPC, Trap 등
    public float interactionRadius = 1.5f;

    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    protected int currentHealth = 0;

    // 함정 해제 관련
    protected int trapClearCount = 0;
    protected GameObject currentTrap = null;
    protected bool isTrapCleared = false;

    // 공격 관련 (공통 데미지 설정)
    [Header("공격 및 스킬 데미지 설정 (공통)")]
    public int basicAttackDamage = 10;
    public int specialAttackDamage = 15;
    public int skillDamage = 30;
    public int ultimateDamage = 50;

    [Header("공격 범위 설정 (공통)")]
    public float basicAttackRange = 1.5f;
    public float specialAttackRange = 1.5f;
    public float skillRange = 2f;
    public float ultimateRange = 3f;

    [Header("쿨타임 설정 (공통)")]
    public float basicAttackCooldown = 0.5f;
    public float specialAttackCooldown = 1f;
    public float skillCooldown = 2f;
    public float ultimateCooldown = 3f;

    [Header("평타 스택 초기화 시간 (공통)")]
    public float basicAttackResetTime = 2f;

    // 공격 상태 및 타이밍 체크
    protected bool isAttacking = false;
    protected int attackIndex = 0;
    protected bool isDead = false;

    protected float lastBasicAttackTime = -Mathf.Infinity;
    protected float lastSpecialAttackTime = -Mathf.Infinity;
    protected float lastSkillTime = -Mathf.Infinity;
    protected float lastUltimateTime = -Mathf.Infinity;

    protected int basicAttackStack = 0;
    protected float lastBasicAttackStackTime = 0f;

    // 이동 입력값 (OnMove에서 업데이트)
    protected Vector2 moveInput;

    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    protected PlayerState currentState = PlayerState.Idle;

    // 애니메이터 및 대쉬
    protected Animator animator;
    protected bool isDashing = false;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (photonView != null && !photonView.IsMine)
        {
            this.enabled = false;
        }
        currentHealth = maxHealth;
        currentState = PlayerState.Idle;
        basicAttackStack = 0;
        lastBasicAttackStackTime = Time.time;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;
        if (isDead || currentState == PlayerState.Death) return;

        CheckDashInput();
        HandleMovement();
        HandleActions();

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
    }

    
    public virtual void OnMove(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        moveInput = context.ReadValue<Vector2>();
    }

    protected virtual void CheckDashInput()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (Time.time - lastDashClickTime <= dashDoubleClickThreshold)
            {
                Vector3 dashDir = new Vector3(moveInput.x, 0, moveInput.y);
                if (dashDir == Vector3.zero)
                    dashDir = transform.forward;
                StartCoroutine(DoDash(dashDir));
                lastDashClickTime = -Mathf.Infinity;
            }
            else
            {
                lastDashClickTime = Time.time;
            }
        }
    }

    protected virtual void HandleMovement()
    {
        if (currentState == PlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);
            if (centerPoint != null)
                centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    protected virtual IEnumerator DoDash(Vector3 dashDir)
    {
        isDashing = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dashDir.normalized * dashDistance;
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

    
    // 공격/스킬 관련 (공통 로직)
   
    protected virtual void HandleActions()
    {
        
    }

    protected virtual void PerformBasicAttack()
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

    protected virtual IEnumerator CoPerformAttack(int index)
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

    protected virtual void PerformSpecialAttack()
    {
        lastSpecialAttackTime = Time.time;
        StartCoroutine(CoPerformAttack(2));
    }

    protected virtual void PerformSkillAttack()
    {
        lastSkillTime = Time.time;
        StartCoroutine(CoPerformAttack(3));
    }

    protected virtual void PerformUltimateAttack()
    {
        lastUltimateTime = Time.time;
        StartCoroutine(CoPerformAttack(4));
    }

    protected virtual void AttackOverlapCheck(float range, int damage)
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
                Debug.Log($"[BasePlayerController] 공격: {damage} 데미지 → 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
    }

    protected virtual void ResetBasicAttackStack()
    {
        basicAttackStack = 0;
        if (animator != null)
            animator.SetInteger("AttackStack", basicAttackStack);
    }

    
    // NPC/Trap 상호작용 및 데미지 처리 (부모에 위치)
   
    public virtual void OnTrapClear(InputAction.CallbackContext context)
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
                Debug.Log($"[BasePlayerController] 함정 키 입력 횟수: {trapClearCount}");

                if (trapClearCount >= 2)
                {
                    isTrapCleared = true;
                    Debug.Log("[BasePlayerController] 함정 해제됨.");
                    Destroy(currentTrap);
                    trapClearCount = 0;
                    currentTrap = null;
                }
            }
        }
    }

    public virtual void OnNPCInteract(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                Debug.Log($"[BasePlayerController] NPC와 상호작용! : {col.name}");
                // 대화 관련 로직은 자식에서 override하여 추가 가능
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"[BasePlayerController] 플레이어 체력: {currentHealth}");

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (isDead) return;
        currentState = PlayerState.Death;
        isDead = true;
        Debug.Log("[BasePlayerController] 플레이어 사망!");

        isAttacking = false;
        attackIndex = 0;
        isDashing = false;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }
}
