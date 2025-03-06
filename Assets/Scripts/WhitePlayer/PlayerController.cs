using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : BasePlayerController
{
    public static PlayerController localPlayer;

    [Header("8방향 이동 속도 설정")]
    public float moveSpeedHorizontal = 5f;
    public float moveSpeedVertical = 5f;
    public float moveSpeedDiagonalLeftUp = 4.5f;
    public float moveSpeedDiagonalRightUp = 4.5f;
    public float moveSpeedDiagonalLeftDown = 4.5f;
    public float moveSpeedDiagonalRightDown = 4.5f;

    [Header("스택 모션 설정")]
    public float comboInputTime = 2f; // 2초 이내 추가 입력 없으면 Idle
    private int attackStack = 0;

    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    [Header("우클릭 Guard/Parry 설정")]
    [Tooltip("우클릭 가드 모션 지속 시간(초) – 무적 시간")]
    public float guardDuration = 2f;
    [Tooltip("우클릭 패링 모션 지속 시간(초) – 무적 시간")]
    public float parryDuration = 2f;
    private bool isGuarding = false;
    private bool isParrying = false;

    [Header("Counter (발도) 설정")]
    [Tooltip("패링 후 좌클릭 시 발도 데미지 (기본값 20)")]
    public int counterDamage = 20;

    // 3D 구 형태의 트리거
    private PlayerAttackZone playerAttackZone;
    private PlayerReceiveDamageZone playerReceiveDamageZone;
    private PlayerInteractionZone playerInteractionZone;

    private PlayerInputActions playerInputActions;

    // 피격 상태 관리 (Bool)
    private bool isHit = false;

    // [Attack_1] 선딜 캔슬 가능 여부
    private bool canStartupCancel = true;

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

        // 자식 오브젝트에 존재하는 Zone들 가져오기
        playerAttackZone = GetComponentInChildren<PlayerAttackZone>();
        playerReceiveDamageZone = GetComponentInChildren<PlayerReceiveDamageZone>();
        playerInteractionZone = GetComponentInChildren<PlayerInteractionZone>();

        if (playerAttackZone == null)
            Debug.LogWarning("[PlayerController] PlayerAttackZone이 자식 오브젝트에 없습니다!");
        if (playerReceiveDamageZone == null)
            Debug.LogWarning("[PlayerController] PlayerReceiveDamageZone이 자식 오브젝트에 없습니다!");
        if (playerInteractionZone == null)
            Debug.LogWarning("[PlayerController] PlayerInteractionZone이 자식 오브젝트에 없습니다!");

        if ((photonView != null && photonView.IsMine) || !PhotonNetwork.InRoom)
            localPlayer = this;
        else
        {
            this.enabled = false;
            return;
        }
    }

    protected override void Update()
    {
        base.Update();

        // 우클릭 입력 처리 (Guard/Parry는 별도 처리)
        if (!isAttacking && !isGuarding && !isParrying && Mouse.current.rightButton.wasPressedThisFrame)
        {
            // 우클릭 입력 시 Guard 실행 (일반 상황)
            StartCoroutine(CoGuardReaction());
        }

        if (isHit || isDead || currentState == PlayerState.Death)
            return;

        HandleActions();

        if (animator != null)
        {
            animator.SetInteger("AttackStack", attackStack);
            animator.SetBool("isAttacking", isAttacking);
        }
    }

    #region 8방향 이동 Override (부모에서 대부분 처리)
    protected override void HandleMovement()
    {
        if (isHit || currentState == PlayerState.Death)
            return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir;
            if (isInVillage)
                moveDir = new Vector3(h, 0, 0).normalized;
            else
            {
                //moveDir = new Vector3(h, 0, v).normalized;
                moveDir = new Vector3(h, v, 0).normalized;
            }
            float speed = GetMoveSpeedByDirection(h, v);
            //transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
            transform.Translate(moveDir * speed * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    private float GetMoveSpeedByDirection(float h, float v)
    {
        if (Mathf.Abs(h) < 0.01f && v > 0.01f) return moveSpeedVertical;
        if (Mathf.Abs(h) < 0.01f && v < -0.01f) return moveSpeedVertical;
        if (h > 0.01f && Mathf.Abs(v) < 0.01f) return moveSpeedHorizontal;
        if (h < -0.01f && Mathf.Abs(v) < 0.01f) return moveSpeedHorizontal;
        if (h < -0.01f && v > 0.01f) return moveSpeedDiagonalLeftUp;
        if (h > 0.01f && v > 0.01f) return moveSpeedDiagonalRightUp;
        if (h < -0.01f && v < -0.01f) return moveSpeedDiagonalLeftDown;
        if (h > 0.01f && v < -0.01f) return moveSpeedDiagonalRightDown;
        return moveSpeedHorizontal;
    }
    #endregion

    #region 공격 & 스킬
    protected override void HandleActions()
    {
        if (isHit || isAttacking || isInVillage) // 마을일경우 공격 불가
            return;

        bool basicAttackInput = playerInputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = playerInputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = playerInputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = playerInputActions.Player.UltimateAttack.triggered;

        // [Attack_1] 선딜 구간 중 캔슬 (Guard/Parry는 우클릭/스페이스바에서 처리됨)
        if (canStartupCancel)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("[Attack_1] 선딜 중 가드로 캔슬");
                CancelAttackAndDoGuard();
                return;
            }
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("[Attack_1] 선딜 중 회피로 캔슬");
                CancelAttackAndDoDodge();
                return;
            }
        }

        if (basicAttackInput && Time.time - lastBasicAttackTime >= 0.5f)
        {
            updateUIAction.Invoke(UIIcon.mouseL, 0.5f);
            lastBasicAttackTime = Time.time;
            FaceMouseDirection();
            StartCoroutine(CoStackAttack());
        }
        else if (specialAttackInput && Time.time - lastSpecialAttackTime >= 1f)
        {
            updateUIAction.Invoke(UIIcon.mouseR, 1f);
            lastSpecialAttackTime = Time.time;
            FaceMouseDirection();
            // 특수공격 로직
        }
        else if (skillAttackInput && Time.time - lastSkillTime >= 2f)
        {
            updateUIAction.Invoke(UIIcon.shift, 2f);
            lastSkillTime = Time.time;
            FaceMouseDirection();
            // 스킬 로직
        }
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= 3f)
        {
            updateUIAction.Invoke(UIIcon.r, 3f);

            lastUltimateTime = Time.time;
            FaceMouseDirection();
            // 궁극기 로직
        }
    }

    private void CancelAttackAndDoGuard()
    {
        ResetStackAndReturnIdle();
        // Guard 상태는 우클릭 입력 시 별도로 처리됩니다.
    }

    private void CancelAttackAndDoDodge()
    {
        ResetStackAndReturnIdle();
        // 회피 로직 (추가 구현 가능)
    }

    private IEnumerator CoStackAttack()
    {
        attackStack++;
        if (attackStack > 4) attackStack = 4;
        updateUIAction.Invoke(UIIcon.mouseLStack, attackStack);

        isAttacking = true;

        // AttackStack=1: Attack_1, 2: Attack_2, 3: Attack_3, 4: Attack_4 등으로 구분
        switch (attackStack)
        {
            case 1: currentState = PlayerState.Attack_L; break;  // Attack_1
            case 2: currentState = PlayerState.Attack_R; break;  // Attack_2
            case 3: currentState = PlayerState.Skill; break;
            case 4: currentState = PlayerState.Ultimate; break;
        }

        yield return new WaitForSeconds(0.2f);

        if (playerAttackZone != null)
        {
            int damage = (attackStack == 1) ? 10 :
                         (attackStack == 2) ? 15 :
                         (attackStack == 3) ? 30 : 50;

            foreach (GameObject enemyObj in playerAttackZone.enemiesInRange)
            {
                //var enemy = enemyObj.GetComponentInParent<EnemyController>();

                // 범위안에 있던 몬스터가 죽으면 null 발생 enemiesInRange에서 제거 해줘야함
                EnemyStateController enemy;
                if (enemyObj != null)
                {
                    enemy = enemyObj.GetComponentInParent<EnemyStateController>();
                    
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        Debug.Log($"[PlayerController] 공격: {damage} 데미지");
                    }
                }

            }
        }

        yield return new WaitForSeconds(0.3f);

        if (attackStack >= 4)
        {
            yield return new WaitForSeconds(0.3f);
            ResetStackAndReturnIdle();
            yield break;
        }

        float timer = 0f;
        bool nextAttack = false;
        while (timer < comboInputTime)
        {
            timer += Time.deltaTime;
            if (moveInput.magnitude > 0.01f || isDashing)
            {
                ResetStackAndReturnIdle();
                yield break;
            }
            if (playerInputActions.Player.BasicAttack.triggered)
            {
                FaceMouseDirection();
                nextAttack = true;
                break;
            }
            yield return null;
        }

        if (!nextAttack)
            ResetStackAndReturnIdle();
        else
            StartCoroutine(CoStackAttack());
    }

    private void ResetStackAndReturnIdle()
    {
        attackStack = 0;
        updateUIAction.Invoke(UIIcon.mouseLStack, attackStack);
        isAttacking = false;
        currentState = PlayerState.Idle;
        canStartupCancel = true;
        if (animator != null)
        {
            animator.SetInteger("AttackStack", 0);
            animator.SetBool("isAttacking", false);
        }
    }
    #endregion

    #region Attack_1 프레임별 처리 (Animation Events)
    public void OnAttack1StartupEnd()
    {
        canStartupCancel = false;
        Debug.Log("[Attack_1] 선딜 종료, 더 이상 선딜 캔슬 불가");
    }

    public void OnAttack1DamageStart()
    {
        Debug.Log("[Attack_1] 공격 판정 시작");
        if (playerAttackZone != null)
            playerAttackZone.EnableAttackCollider(true);
        //transform.Translate(Vector3.forward * 1.0f, Space.Self);
    }

    public void OnAttack1DamageEnd()
    {
        Debug.Log("[Attack_1] 공격 판정 종료");
        if (playerAttackZone != null)
            playerAttackZone.EnableAttackCollider(false);
    }

    public void OnAttack1AllowNextInput()
    {
        Debug.Log("[Attack_1] 다음 평타(Attack_2) 입력 가능");
    }

    public void OnAttack1RecoveryEnd()
    {
        Debug.Log("[Attack_1] 후딜 끝, 모든 행동 가능");
    }

    public void OnAttack1AnimationEnd()
    {
        Debug.Log("[Attack_1] 애니메이션 완전 종료");
        if (currentState == PlayerState.Attack_L)
            ResetStackAndReturnIdle();
    }
    #endregion

    #region Attack_2 프레임별 처리 (Animation Events)
    public void OnAttack2StartupFrame1End()
    {
        Debug.Log("[Attack_2] 1프레임 종료 시 x=0.7 전진");
        //transform.Translate(Vector3.forward * 0.7f, Space.Self);
    }

    public void OnAttack2StartupFrame2End()
    {
        Debug.Log("[Attack_2] 2프레임 종료 시 x=0.7 전진");
        //transform.Translate(Vector3.forward * 0.7f, Space.Self);
    }

    public void OnAttack2DamageStart()
    {
        Debug.Log("[Attack_2] 공격 판정 시작");
        if (playerAttackZone != null)
            playerAttackZone.EnableAttackCollider(true);
    }

    public void OnAttack2DamageEnd()
    {
        Debug.Log("[Attack_2] 공격 판정 종료");
        if (playerAttackZone != null)
            playerAttackZone.EnableAttackCollider(false);
    }

    public void OnAttack2AllowNextInput()
    {
        Debug.Log("[Attack_2] 추가 평타(Attack_3) 입력 가능");
    }

    public void OnAttack2RecoveryEnd()
    {
        Debug.Log("[Attack_2] 후딜 끝, 이동/회피/스킬 등 자유롭게 가능");
    }

    public void OnAttack2AnimationEnd()
    {
        Debug.Log("[Attack_2] 애니메이션 완전 종료");
        if (currentState == PlayerState.Attack_R)
            ResetStackAndReturnIdle();
    }
    #endregion

    #region 우클릭 Guard/Parry 처리
    private IEnumerator CoGuardReaction()
    {
        isGuarding = true;
        currentState = PlayerState.Idle; // Guard는 상태 전환은 Animator 파라미터로 처리
        if (animator != null)
        {
            animator.SetBool("isGuard", true);
        }
        Debug.Log("[Guard] Guard 시작, 무적 상태");

        // Guard 모션 중에 이동 입력이 감지되면 조기 종료 (이때 Guard 애니메이션이 끝나도록 함)
        float elapsed = 0f;
        while (elapsed < guardDuration)
        {
            if (moveInput.magnitude > 0.1f)
            {
                Debug.Log("[Guard] 이동 입력 감지, Guard 종료");
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        isGuarding = false;
        if (animator != null)
        {
            animator.SetBool("isGuard", false);
        }
        Debug.Log("[Guard] Guard 종료");
    }

    private IEnumerator CoParryReaction()
    {
        isParrying = true;
        currentState = PlayerState.Idle; // Parry 상태는 Animator 파라미터로 처리
        if (animator != null)
        {
            animator.SetBool("isParry", true);
        }
        Debug.Log("[Parry] Parry 시작, 무적 상태");
        float elapsed = 0f;
        bool counterTriggered = false;
        while (elapsed < parryDuration)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("[Parry] Counter Attack (발도) triggered");
                OnCounterAttackEvent();
                counterTriggered = true;
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (animator != null)
        {
            animator.SetBool("isParry", false);
        }
        isParrying = false;
        Debug.Log("[Parry] Parry 종료");
    }

    /// <summary>
    /// 발도 모션 이벤트 함수: 패링 모션 도중 좌클릭 이벤트에 의해 발도(반격) 데미지 발생
    /// 이 함수는 애니메이션 이벤트로 호출될 수 있으며, 또는 코루틴 내에서도 호출 가능합니다.
    /// </summary>
    public void OnCounterAttackEvent()
    {
        int damage = counterDamage; // 인스펙터에서 조정 가능
        if (playerAttackZone != null)
        {
            foreach (GameObject enemyObj in playerAttackZone.enemiesInRange)
            {
                //var enemy = enemyObj.GetComponentInParent<EnemyController>();
                //if (enemy != null)
                //{
                //    enemy.TakeDamage(damage);
                //    Debug.Log("[CounterAttack] 적에게 " + damage + " 데미지");
                //}
            }
        }
    }
    #endregion

    #region 피격 (Hit) 처리
    public override void TakeDamage(int damage)
    {
        if (isGuarding || isParrying)
            return;

        // 우클릭 입력 시, 피해 대신 Parry 처리
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            StartCoroutine(CoParryReaction());
            return;
        }

        base.TakeDamage(damage);
        if (isDead || currentState == PlayerState.Death)
            return;
        if (!isHit)
            StartCoroutine(CoHitReaction());
    }

    private IEnumerator CoHitReaction()
    {
        isHit = true;
        currentState = PlayerState.Hit;
        if (animator != null)
            animator.SetBool("isHit", true);
        yield return new WaitForSeconds(0.5f);
        isHit = false;
        if (animator != null)
            animator.SetBool("isHit", false);
        if (!isDead)
            currentState = PlayerState.Idle;
    }
    #endregion

    #region 마우스 방향 바라보기
    private void FaceMouseDirection()
    {
        /*
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        if (Camera.main == null)
            return;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.forward = lookDir.normalized;
        }
        */
    }
    #endregion
}
