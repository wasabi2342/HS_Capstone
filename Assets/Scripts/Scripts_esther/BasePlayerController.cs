using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// 모든 플레이어가 공통으로 사용하는 이동, 대쉬 등의 로직을 담은 부모 클래스

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

    // 이동/대쉬 상태
    protected bool isDashing = false;

    // 이동 입력값 
    protected Vector2 moveInput;

    // 플레이어 상태 예시
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    protected PlayerState currentState = PlayerState.Idle;

    // 이동 애니메이션용 Animator
    protected Animator animator;

    protected virtual void Awake()
    {
        // 현재 GameObject(또는 자식)에 Animator가 있다면 가져옵니다
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        // PhotonView 소유권 체크
        if (photonView != null && !photonView.IsMine)
        {
            this.enabled = false; // 다른 플레이어의 컨트롤러는 비활성화
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

        // 스페이스바 더블탭으로 대쉬 입력 체크
        CheckDashInput();

        // 이동 처리
        HandleMovement();
    }

   
    // 스페이스바를 두 번 누르면 대쉬
  
    protected virtual void CheckDashInput()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 더블탭 시간 내에 다시 누르면 대쉬
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

  
    // 이동 처리 (Idle/Run 상태 전환 + 애니메이터 파라미터)
    // 자식에서 8방향 등으로 오버라이드할 수 있음
  
    protected virtual void HandleMovement()
    {
        if (currentState == PlayerState.Death) return; // 사망 시 이동 불가

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);

        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            // 기본(수평/수직) 이동
            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);

            // 중심점 업데이트
            if (centerPoint != null)
                centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }

        // 기본 애니메이터 파라미터
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    /// <summary>
    /// 대쉬 코루틴
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
