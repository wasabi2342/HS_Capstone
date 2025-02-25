using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public abstract class BasePlayerController : MonoBehaviourPunCallbacks
{
    [Header("이동 속도 설정")]
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

    // 이동 입력값
    protected Vector2 moveInput;

    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    protected PlayerState currentState = PlayerState.Idle;

    
    protected Animator animator;

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
    }


    public virtual void OnMove(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        moveInput = context.ReadValue<Vector2>();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;
        CheckDashInput();
        HandleMovement();
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

   
    // 기본 이동 처리 ? 자식에서 override하여 확장 가능.
    
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

   
    // 대쉬 코루틴 ? virtual로 선언하여 자식에서 override할 수 있음.
    
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

    // Base 클래스에서 사용하는 대쉬 플래그
    protected bool isDashing = false;
}
