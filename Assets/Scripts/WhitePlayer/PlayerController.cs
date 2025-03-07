using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
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

    //공격 관련
    [Header("공격/콤보 설정")]
    public float comboInputTime = 2f; // 추가 입력 허용 시간
    private int attackStack = 0;
    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;
    private bool isAttacking = false;
    private bool canStartupCancel = true;  // 공격 선딜 캔슬 여부

    [Header("우클릭 가드/패링 설정")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;
    private bool isGuarding = false;
    private bool isParrying = false;

    [Header("Counter (발도) 설정")]
    public int counterDamage = 20;

    // 참조 컴포넌트 
    private Animator animator;
    // 공격 판정용 영역, 상호작용 영역 등은 필요에 따라 추가
    // private PlayerAttackZone playerAttackZone;  // (생략 가능)

    //  기타 
    // UI 업데이트, 인풋 액션 등은  상황에 맞게 추가
    // public System.Action<string, float> updateUIAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
        // 공격, 스킬 등은 이벤트 호출(WhitePlayercontroller_event.cs)로 실행됩니다.
    }

    //  기본 이동 관련
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

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
            // 예시로 마을에서는 수평 이동만, 전투 시 2D 평면 이동 처리
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
        // 8방향 centerPoint 갱신
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

    //상호작용 관련 
    public void OnNPCInteract(InputAction.CallbackContext context)
    {
        // 상호작용 로직 (예: NPC, 포탈, 훈련소 등)
        Debug.Log("NPC 상호작용 호출됨.");
        // 필요에 따라 구체적인 처리 코드를 추가합니다.
    }

    //공격/스킬 관련

    // 평타 (기본 공격)
    public void HandleNormalAttack()
    {
        if (Time.time - lastBasicAttackTime >= 0.5f && !isAttacking)
        {
            lastBasicAttackTime = Time.time;
            FaceMouseDirection();
            StartCoroutine(CoStackAttack());
        }
    }

    // 특수 공격 (평타보다 강한 공격)
    public void HandleSpecialAttack()
    {
        if (Time.time - lastSpecialAttackTime >= 1f && !isAttacking)
        {
            lastSpecialAttackTime = Time.time;
            FaceMouseDirection();
            // 특수 공격 로직 구현 (예: 애니메이션 재생 및 데미지 적용)
            Debug.Log("특수 공격 실행");
        }
    }

    // 궁극기 공격
    public void HandleUltimateAttack()
    {
        if (Time.time - lastUltimateTime >= 3f && !isAttacking)
        {
            lastUltimateTime = Time.time;
            FaceMouseDirection();
            // 궁극기 로직 구현 (예: 애니메이션 재생 및 데미지 적용)
            Debug.Log("궁극기 공격 실행");
        }
    }

    // 가드 처리 (우클릭 시)
    public void HandleGuard()
    {
        if (!isAttacking && !isGuarding && !isParrying)
        {
            StartCoroutine(CoGuardReaction());
        }
    }

    // 패링 처리 (Guard 중 좌클릭 등으로)
    public void HandleParry()
    {
        if (!isParrying)
        {
            StartCoroutine(CoParryReaction());
        }
    }

    private IEnumerator CoStackAttack()
    {
        attackStack++;
        if (attackStack > 4) attackStack = 4;
        isAttacking = true;
        // 공격 스택에 따라 상태 전환 (Attack_L, Attack_R, Skill, Ultimate)
        switch (attackStack)
        {
            case 1: currentState = PlayerState.Attack_L; break;
            case 2: currentState = PlayerState.Attack_R; break;
            case 3: currentState = PlayerState.Skill; break;
            case 4: currentState = PlayerState.Ultimate; break;
        }

        // 실제 공격 판정은 애니메이션 이벤트 (WhitePlayerController_AttackStack.cs)에서 호출합니다.
        yield return new WaitForSeconds(0.2f);

        // 추가 입력 대기 시간 (콤보)
        float timer = 0f;
        while (timer < comboInputTime)
        {
            timer += Time.deltaTime;
            if (moveInput.magnitude > 0.01f || isDashing)
            {
                ResetAttackStack();
                yield break;
            }
            yield return null;
        }
        ResetAttackStack();
    }

    private void ResetAttackStack()
    {
        attackStack = 0;
        isAttacking = false;
        currentState = PlayerState.Idle;
        canStartupCancel = true;
    }

    
    public void OnAttack1StartupEnd()
    {
        canStartupCancel = false;
        Debug.Log("Attack_1 선딜 종료");
    }
    public void OnAttack1DamageStart()
    {
        Debug.Log("Attack_1 공격 판정 시작");
        // 공격 판정 활성화 
    }
    public void OnAttack1DamageEnd()
    {
        Debug.Log("Attack_1 공격 판정 종료");
        // 공격 판정 비활성화
    }
    public void OnAttack1AllowNextInput()
    {
        Debug.Log("Attack_1 추가 입력 허용");
    }
    public void OnAttack1RecoveryEnd()
    {
        Debug.Log("Attack_1 후딜 종료");
    }
    public void OnAttack1AnimationEnd()
    {
        Debug.Log("Attack_1 애니메이션 종료");
        ResetAttackStack();
    }

    public void OnAttack2StartupFrame1End()
    {
        Debug.Log("Attack_2 1프레임 종료 (전진 효과)");
    }
    public void OnAttack2StartupFrame2End()
    {
        Debug.Log("Attack_2 2프레임 종료 (전진 효과)");
    }
    public void OnAttack2DamageStart()
    {
        Debug.Log("Attack_2 공격 판정 시작");
    }
    public void OnAttack2DamageEnd()
    {
        Debug.Log("Attack_2 공격 판정 종료");
    }
    public void OnAttack2AllowNextInput()
    {
        Debug.Log("Attack_2 추가 입력 허용");
    }
    public void OnAttack2RecoveryEnd()
    {
        Debug.Log("Attack_2 후딜 종료");
    }
    public void OnAttack2AnimationEnd()
    {
        Debug.Log("Attack_2 애니메이션 종료");
        ResetAttackStack();
    }

    //가드 및 패링 관련 코루틴
    private IEnumerator CoGuardReaction()
    {
        isGuarding = true;
        currentState = PlayerState.Guard;
        if (animator != null)
            animator.SetBool("isGuard", true);
        Debug.Log("Guard 시작 (무적 상태)");

        float elapsed = 0f;
        while (elapsed < guardDuration)
        {
            if (moveInput.magnitude > 0.1f)
            {
                Debug.Log("이동 입력 감지 - Guard 종료");
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        isGuarding = false;
        if (animator != null)
            animator.SetBool("isGuard", false);
        Debug.Log("Guard 종료");
    }

    private IEnumerator CoParryReaction()
    {
        isParrying = true;
        currentState = PlayerState.Parry;
        if (animator != null)
            animator.SetBool("isParry", true);
        Debug.Log("Parry 시작 (무적 상태)");

        float elapsed = 0f;
        while (elapsed < parryDuration)
        {
            
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("패링 중 카운터 공격 트리거");
                OnCounterAttackEvent();
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (animator != null)
            animator.SetBool("isParry", false);
        isParrying = false;
        Debug.Log("Parry 종료");
    }

    // 발도(반격) 이벤트 함수 – 애니메이션 이벤트나 코루틴 내에서 호출
    public void OnCounterAttackEvent()
    {
        Debug.Log("Counter Attack (발도) 실행, 데미지: " + counterDamage);
        // 반격 시, 적에게 counterDamage 만큼의 데미지를 적용하는 로직 구현
    }

    //  피격 및 사망 처리 
    public void TakeDamage(int damage)
    {
        if (isGuarding || isParrying)
            return;

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

    // 씬 전환 관련 (예: 포탈, 훈련실)
   
    public void UsePortal(Vector3 exitPosition)
    {
        transform.position = exitPosition;
        // 포탈 사용 후 쿨다운 처리 등 추가
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 후 초기화 처리 등
    }
}
