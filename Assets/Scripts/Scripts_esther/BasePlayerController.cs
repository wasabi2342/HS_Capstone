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

    [Header("상호작용 / 데미지 범위 설정")]
    public LayerMask interactionLayerMask;
    public float interactionRadius = 1.5f;

    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    protected int currentHealth = 0;

    // 함정 해제 관련
    protected int trapClearCount = 0;
    protected GameObject currentTrap = null;
    protected bool isTrapCleared = false;

    // 공격/데미지 관련
    protected bool isAttacking = false;
    protected bool isDead = false;

    // 이동 입력
    protected Vector2 moveInput;

    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    protected PlayerState currentState = PlayerState.Idle;

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
        HandleActions(); // 자식에서 override (공격/스킬 로직)

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
    }

    /// <summary>
    /// 새 Input 시스템에서 "Move" 액션 → Invoke Unity Events
    /// </summary>
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

    /// <summary>
    /// 자식에서 override하여 공격/스킬 로직 처리
    /// </summary>
    protected virtual void HandleActions()
    {
        // 자식에서 구현
    }

    // --------------------------------------------------------------------
    // 상호작용 (Trap/NPC)
    // --------------------------------------------------------------------
    public virtual void OnTrapClear(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        if (centerPoint == null) return;
        Vector3 checkPos = centerPoint.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Trap"))
            {
                trapClearCount++;
                Debug.Log($"[BasePlayerController] 함정 키 입력 횟수: {trapClearCount}");
                if (trapClearCount >= 2)
                {
                    isTrapCleared = true;
                    Debug.Log("[BasePlayerController] 함정 해제됨.");
                    Destroy(col.gameObject);
                    trapClearCount = 0;
                }
            }
        }
    }

    public virtual void OnNPCInteract(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        if (centerPoint == null) return;
        Vector3 checkPos = centerPoint.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                Debug.Log($"[BasePlayerController] NPC와 상호작용! : {col.name}");
            }
        }
    }

    // --------------------------------------------------------------------
    // 데미지 처리
    // --------------------------------------------------------------------
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
        isDashing = false;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }
}
