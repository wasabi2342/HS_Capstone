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
    [Tooltip("기본 CenterPoint. Invoke 이벤트 등에서 사용.")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("8방향 CenterPoints 설정")]
    [Tooltip("플레이어의 8방향 CenterPoint들을 할당 (순서: 0=위, 1=우상, 2=오른쪽, 3=우하, 4=아래, 5=좌하, 6=왼쪽, 7=좌상)")]
    public Transform[] centerPoints = new Transform[8];
    public int currentDirectionIndex = 0;

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

    // PlayerState 열거형에 Guard와 Parry 상태 추가 (추가: Guard, Parry)
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Hit, Guard, Parry, Death }
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

        // 8방향 CenterPoint 갱신
        if (centerPoints != null && centerPoints.Length >= 8)
        {
            if (moveInput.magnitude > 0.01f)
            {
                currentDirectionIndex = DetermineDirectionIndex(moveInput);
            }
            centerPoint.position = centerPoints[currentDirectionIndex].position;
        }
        else
        {
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }

        CheckDashInput();
        HandleMovement();
        HandleActions();
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

    protected virtual void HandleActions()
    {
        // 자식에서 구현
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

    protected int DetermineDirectionIndex(Vector2 input)
    {
        if (input.magnitude < 0.01f) return currentDirectionIndex;
        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int idx = Mathf.RoundToInt(angle / 45f) % 8;
        return idx;
    }
}
