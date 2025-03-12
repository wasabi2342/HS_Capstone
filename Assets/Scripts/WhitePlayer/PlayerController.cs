using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //  기본 이동 및 체력 관련 
    [Header("이동 속도")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("대쉬 설정")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    private float lastDashClickTime = -Mathf.Infinity;
    private bool isDashing = false;

    [Header("중심점 설정")]
    [Tooltip("기본 CenterPoint (애니메이션 이벤트 등에서 사용)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8방향 CenterPoint 배열 (순서: 0=위, 1=우상, 2=오른쪽, 3=우하, 4=아래, 5=좌하, 6=왼쪽, 7=좌상)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    [Header("플레이어 체력")]
    public int maxHealth = 100;
    private int currentHealth;

    // 이동 입력 및 상태
    private Vector2 moveInput;
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Hit, Guard, Parry, Death }
    private PlayerState currentState = PlayerState.Idle;

    // 공격/콤보 설정 (AttackStack 파라미터 사용)
    [Header("공격/콤보 설정")]
    [Tooltip("콤보 다음 입력을 기다리는 시간(초) (2초 내에 좌클릭해야 다음 스택으로 넘어감)")]
    public float comboInputTime = 2f;
    private int attackStack = 0;         // 1~4단계 콤보
    private bool isAttacking = false;
    private bool canStartupCancel = true;  // 공격 선딜 캔슬 여부 (필요 시 사용)

    [Header("우클릭 가드/패링 설정")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;
    private bool isGuarding = false;
    private bool isParrying = false;

    [Header("Counter (발도) 설정")]
    public int counterDamage = 20;

    // 참조 컴포넌트 
    private Animator animator;

    private void Awake()
    {

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다! (WhitePlayerController)");
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        currentState = PlayerState.Idle;
    }

    private void Update()
    {
        if (currentState == PlayerState.Death)
            return;

        UpdateCenterPoint();
        CheckDashInput();
        HandleMovement();
        // 공격/스킬, 가드 등은 별도 스크립트에서 호출
    }

    // 입력 처리 관련
    // WhitePlayercontroller_event.cs에서 호출하여 이동 입력을 설정
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    // === 이동 처리 ===
    private void HandleMovement()
    {
        if (currentState == PlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir;
            moveDir = (Mathf.Abs(v) > 0.01f) ? new Vector3(h, 0, v).normalized : new Vector3(h, 0, 0).normalized;
            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    private void UpdateCenterPoint()
    {
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
    }

    private int DetermineDirectionIndex(Vector2 input)
    {
        if (input.magnitude < 0.01f)
            return currentDirectionIndex;
        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int idx = Mathf.RoundToInt(angle / 45f) % 8;
        return idx;
    }

    // === 대쉬 처리 ===
    private void CheckDashInput()
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

    private IEnumerator DoDash(Vector3 dashDir)
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
    }

    // 평타 공격(1→2→3→4단계)
    public void HandleNormalAttack()
    {
        // 이미 공격 중이 아니라면 콤보 시작
        if (!isAttacking)
        {
            StartCoroutine(CoStackAttack());
        }
    }


    // 1~4단계 콤보를 순차적으로 실행하는 코루틴
    // 각 단계가 끝난 후 2초 내에 마우스 좌클릭이 없으면 Idle로 복귀
    // 4단계가 끝나면 즉시 Idle로 복귀

    private IEnumerator CoStackAttack()
    {
        isAttacking = true;

        // 1~4단계를 순차적으로 진행
        for (int stage = 1; stage <= 4; stage++)
        {
            attackStack = stage;
            switch (stage)
            {
                case 1:
                    currentState = PlayerState.Attack_L;
                    Debug.Log("평타 공격 스택 1단계 실행");
                    break;
                case 2:
                    currentState = PlayerState.Attack_R;
                    Debug.Log("평타 공격 스택 2단계 실행");
                    break;
                case 3:
                    currentState = PlayerState.Skill;
                    Debug.Log("평타 공격 스택 3단계 실행");
                    break;
                case 4:
                    currentState = PlayerState.Ultimate;
                    Debug.Log("평타 공격 스택 4단계 실행");
                    break;
            }

            // 마우스 방향 보기 (필요하다면)
            FaceMouseDirection();

            // Animator 파라미터 설정
            if (animator != null)
            {
                animator.SetInteger("AttackStack", stage);
                animator.SetBool("isAttacking", true);
            }

            // 약간의 선딜(0.2초) 대기 
            yield return new WaitForSeconds(0.2f);

            // 만약 현재가 4단계라면 바로 종료
            if (stage == 4)
            {
                // 4단계 애니메이션이 끝나면 즉시 Idle로 돌아가도록 처리
                break;
            }

            // 다음 단계로 넘어가기 위해 2초 동안 좌클릭을 기다림
            float timer = 0f;
            bool nextClick = false;
            while (timer < comboInputTime)
            {
                timer += Time.deltaTime;

                // 이동/대쉬가 발생하면 콤보 끊기
                if (moveInput.magnitude > 0.01f || isDashing)
                {
                    ResetAttackStack();
                    yield break;
                }

                // 마우스 좌클릭 감지
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    nextClick = true;
                    break;
                }
                yield return null;
            }

            // 만약 2초 안에 좌클릭이 없었다면 콤보 끊기
            if (!nextClick)
            {
                ResetAttackStack();
                yield break;
            }

            // 좌클릭이 있었다면 for 루프 다음 단계로 넘어감
        }

        // 4단계까지 완료했거나 루프가 끝났으므로 종료
        ResetAttackStack();
    }

    private void ResetAttackStack()
    {
        attackStack = 0;
        isAttacking = false;
        currentState = PlayerState.Idle;
        canStartupCancel = true;
        Debug.Log("평타 공격 스택 초기화 → Idle 상태로 복귀");

        if (animator != null)
        {
            animator.SetInteger("AttackStack", 0);
            animator.SetBool("isAttacking", false);
        }
    }

    // 특수 공격
    public void HandleSpecialAttack()
    {
        FaceMouseDirection();
        Debug.Log("특수 공격 호출됨");
        if (animator != null)
        {
            animator.SetTrigger("SpecialAttack");
        }
    }

    // 궁극기 공격 
    public void HandleUltimateAttack()
    {
        FaceMouseDirection();
        Debug.Log("궁극기 공격 호출됨");
        if (animator != null)
        {
            animator.SetTrigger("UltimateAttack");
        }
    }

    // 공격 애니메이션 이벤트용 스텁 (WhitePlayerController_AttackStack에서 호출) 
    public void OnAttack1StartupEnd() { Debug.Log("공격 1: 선딜 종료"); }
    public void OnAttack1DamageStart() { Debug.Log("공격 1: 데미지 시작"); }
    public void OnAttack1DamageEnd() { Debug.Log("공격 1: 데미지 종료"); }
    public void OnAttack1AllowNextInput() { Debug.Log("공격 1: 추가 입력 허용"); }
    public void OnAttack1RecoveryEnd() { Debug.Log("공격 1: 후딜 종료"); }
    public void OnAttack1AnimationEnd() { Debug.Log("공격 1: 애니메이션 종료"); }

    public void OnAttack2StartupFrame1End() { Debug.Log("공격 2: 스타트업 프레임 1 종료"); }
    public void OnAttack2StartupFrame2End() { Debug.Log("공격 2: 스타트업 프레임 2 종료"); }
    public void OnAttack2DamageStart() { Debug.Log("공격 2: 데미지 시작"); }
    public void OnAttack2DamageEnd() { Debug.Log("공격 2: 데미지 종료"); }
    public void OnAttack2AllowNextInput() { Debug.Log("공격 2: 추가 입력 허용"); }
    public void OnAttack2RecoveryEnd() { Debug.Log("공격 2: 후딜 종료"); }
    public void OnAttack2AnimationEnd() { Debug.Log("공격 2: 애니메이션 종료"); }

    // 가드/패링 처리
    public void HandleGuard()
    {
        Debug.Log("가드 호출됨");

    }

    public void HandleParry()
    {
        Debug.Log("패링 호출됨");
    }

    public void OnCounterAttackEvent()
    {
        Debug.Log("발도(반격) 호출됨, 데미지: " + counterDamage);
    }

    // 피격 및 사망 처리
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("플레이어 체력: " + currentHealth);

        if (currentHealth <= 0 && currentState != PlayerState.Death)
        {
            Die();
        }
        else
        {
            StartCoroutine(CoHitReaction());
        }
    }

    private IEnumerator CoHitReaction()
    {
        currentState = PlayerState.Hit;
        if (animator != null)
            animator.SetBool("isHit", true);
        yield return new WaitForSeconds(0.5f);
        if (animator != null)
            animator.SetBool("isHit", false);
        if (currentState != PlayerState.Death)
            currentState = PlayerState.Idle;
    }

    private void Die()
    {
        currentState = PlayerState.Death;
        Debug.Log("플레이어 사망");
        if (animator != null)
            animator.SetBool("isDead", true);
    }

    // 기타 유틸리티
    private void FaceMouseDirection()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (Camera.main == null)
            return;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.forward = lookDir.normalized;
        }
    }

    // 씬 전환 관련
    public void UsePortal(Vector3 exitPosition)
    {
        transform.position = exitPosition;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 후 초기화 처리
    }
}
