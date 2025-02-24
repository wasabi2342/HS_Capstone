using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// ��� �÷��̾ �������� ����ϴ� �̵�, �뽬 ���� ������ ���� �θ� Ŭ����

public abstract class BasePlayerController : MonoBehaviourPunCallbacks
{
    [Header("�̵� �ӵ� ����")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("�뽬 ����")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    protected float lastDashClickTime = -Mathf.Infinity;

    [Header("�߽��� ����")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    // �̵�/�뽬 ����
    protected bool isDashing = false;

    // �̵� �Է°� 
    protected Vector2 moveInput;

    // �÷��̾� ���� ����
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    protected PlayerState currentState = PlayerState.Idle;

    // �̵� �ִϸ��̼ǿ� Animator
    protected Animator animator;

    protected virtual void Awake()
    {
        // ���� GameObject(�Ǵ� �ڽ�)�� Animator�� �ִٸ� �����ɴϴ�
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // PhotonView ������ üũ
        if (photonView != null && !photonView.IsMine)
        {
            this.enabled = false; // �ٸ� �÷��̾��� ��Ʈ�ѷ��� ��Ȱ��ȭ
        }
    }

 
    public virtual void OnMove(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        moveInput = context.ReadValue<Vector2>();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;

        // �����̽��� ���������� �뽬 �Է� üũ
        CheckDashInput();

        // �̵� ó��
        HandleMovement();
    }

   
    // �����̽��ٸ� �� �� ������ �뽬
  
    protected virtual void CheckDashInput()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // ������ �ð� ���� �ٽ� ������ �뽬
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

  
    // �̵� ó�� (Idle/Run ���� ��ȯ + �ִϸ����� �Ķ����)
    // �ڽĿ��� 8���� ������ �������̵��� �� ����
  
    protected virtual void HandleMovement()
    {
        if (currentState == PlayerState.Death) return; // ��� �� �̵� �Ұ�

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);

        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            // �⺻(����/����) �̵�
            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);

            // �߽��� ������Ʈ
            if (centerPoint != null)
                centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }

        // �⺻ �ִϸ����� �Ķ����
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    /// <summary>
    /// �뽬 �ڷ�ƾ
    /// </summary>
    protected IEnumerator DoDash(Vector3 dashDir)
    {
        isDashing = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dashDir.normalized * dashDistance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        isDashing = false;
    }
}
