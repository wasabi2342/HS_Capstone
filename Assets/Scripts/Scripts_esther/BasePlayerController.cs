using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public abstract class BasePlayerController : MonoBehaviourPunCallbacks
{
    [Header("�⺻ �̵� �ӵ�")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("�뽬 ����")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    protected float lastDashClickTime = -Mathf.Infinity;

    [Header("�߽��� ����")]
    [Tooltip("�⺻ CenterPoint. Invoke �̺�Ʈ ��� ���.")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("8���� CenterPoints ����")]
    [Tooltip("�÷��̾��� 8���� CenterPoint���� �Ҵ� (����: 0=��, 1=���, 2=������, 3=����, 4=�Ʒ�, 5=����, 6=����, 7=�»�)")]
    public Transform[] centerPoints = new Transform[8];
    public int currentDirectionIndex = 0;

    [Header("��ȣ�ۿ� / ������ ���� ����")]
    public LayerMask interactionLayerMask;
    public float interactionRadius = 1.5f;

    [Header("�÷��̾� ü�� ����")]
    public int maxHealth = 100;
    protected int currentHealth = 0;

    // ���� ���� ����
    protected int trapClearCount = 0;
    protected GameObject currentTrap = null;
    protected bool isTrapCleared = false;

    // ����/������ ����
    protected bool isAttacking = false;
    protected bool isDead = false;

    // �̵� �Է�
    protected Vector2 moveInput;

    // PlayerState �������� Guard�� Parry ���� �߰� (�߰�: Guard, Parry)
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

        // 8���� CenterPoint ����
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
        // �ڽĿ��� ����
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
                Debug.Log($"[BasePlayerController] NPC�� ��ȣ�ۿ�! : {col.name}");
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
                Debug.Log($"[BasePlayerController] ���� Ű �Է� Ƚ��: {trapClearCount}");
                if (trapClearCount >= 2)
                {
                    isTrapCleared = true;
                    Debug.Log("[BasePlayerController] ���� ������.");
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
        Debug.Log($"[BasePlayerController] �÷��̾� ü��: {currentHealth}");

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
        Debug.Log("[BasePlayerController] �÷��̾� ���!");

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
