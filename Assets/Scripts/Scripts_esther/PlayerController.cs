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

    // 3D 구 형태의 트리거
    private PlayerAttackZone playerAttackZone;            // 플레이어가 적을 공격
    private PlayerReceiveDamageZone playerReceiveDamageZone;  // 플레이어가 적에게 맞음
    private PlayerInteractionZone playerInteractionZone;  // 플레이어가 상호작용

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

        // 혹시 없으면 경고
        if (playerAttackZone == null)
        {
            Debug.LogWarning("[PlayerController] PlayerAttackZone이 자식 오브젝트에 없습니다!");
        }
        if (playerReceiveDamageZone == null)
        {
            Debug.LogWarning("[PlayerController] PlayerReceiveDamageZone이 자식 오브젝트에 없습니다!");
        }
        if (playerInteractionZone == null)
        {
            Debug.LogWarning("[PlayerController] PlayerInteractionZone이 자식 오브젝트에 없습니다!");
        }

        // 로컬 플레이어 체크
        if (photonView != null && photonView.IsMine)
        {
            localPlayer = this;
        }
        else
        {
            this.enabled = false;
            return;
        }
    }

    protected override void Update()
    {
        base.Update();

        // 피격 중, 사망 시 행동 중단
        if (isHit || isDead || currentState == PlayerState.Death)
            return;

        // 공격/스킬 입력
        HandleActions();

        // 애니메이터 파라미터 동기화
        if (animator != null)
        {
            animator.SetInteger("AttackStack", attackStack);
            animator.SetBool("isAttacking", isAttacking);
        }
    }

    #region 8방향 이동 Override (부모에서 대부분 처리)
    protected override void HandleMovement()
    {
        // 피격 중이거나 사망 시 이동 불가
        if (isHit || currentState == PlayerState.Death)
            return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            float speed = GetMoveSpeedByDirection(h, v);
            transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
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
        // 피격 중 or 이미 공격 중이면 행동 불가
        if (isHit || isAttacking)
            return;

        bool basicAttackInput = playerInputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = playerInputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = playerInputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = playerInputActions.Player.UltimateAttack.triggered;

        // [1] 선딜 구간 중에만 캔슬 가능
        if (canStartupCancel)
        {
            // 우클릭 = Guard
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("[Attack_1] 선딜 중 가드로 캔슬");
                CancelAttackAndDoGuard();
                return;
            }
            // 스페이스바 = 회피
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Debug.Log("[Attack_1] 선딜 중 회피로 캔슬");
                CancelAttackAndDoDodge();
                return;
            }
        }

        // 기본 공격(마우스 좌클릭)
        if (basicAttackInput && Time.time - lastBasicAttackTime >= 0.5f)
        {
            lastBasicAttackTime = Time.time;
            FaceMouseDirection(); // 공격 전 마우스 방향 바라보기
            StartCoroutine(CoStackAttack());
        }
        // 특수공격
        else if (specialAttackInput && Time.time - lastSpecialAttackTime >= 1f)
        {
            lastSpecialAttackTime = Time.time;
            FaceMouseDirection();
            // 특수공격 로직
        }
        // 스킬
        else if (skillAttackInput && Time.time - lastSkillTime >= 2f)
        {
            lastSkillTime = Time.time;
            FaceMouseDirection();
            // 스킬 로직
        }
        // 궁극기
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= 3f)
        {
            lastUltimateTime = Time.time;
            FaceMouseDirection();
            // 궁극기 로직
        }
    }

    private void CancelAttackAndDoGuard()
    {
        ResetStackAndReturnIdle();
        // 가드 상태 
        // animator.SetBool("isGuard", true);
    }

    private void CancelAttackAndDoDodge()
    {
        ResetStackAndReturnIdle();
        // 회피 로직 
       
    }

    private IEnumerator CoStackAttack()
    {
        attackStack++;
        if (attackStack > 4) attackStack = 4;

        isAttacking = true;

        switch (attackStack)
        {
            case 1: currentState = PlayerState.Attack_L; break;
            case 2: currentState = PlayerState.Attack_R; break;
            case 3: currentState = PlayerState.Skill; break;
            case 4: currentState = PlayerState.Ultimate; break;
        }

        yield return new WaitForSeconds(0.2f);

        // 근접 공격 범위 내 적에게 데미지 적용
        if (playerAttackZone != null)
        {
            int damage = (attackStack == 1) ? 10 :
                         (attackStack == 2) ? 15 :
                         (attackStack == 3) ? 30 : 50;

            // playerAttackZone 내부의 적에게 데미지
            foreach (GameObject enemyObj in playerAttackZone.enemiesInRange)
            {
                var enemy = enemyObj.GetComponentInParent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[PlayerController] 공격: {damage} 데미지");
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        // 스택=4이면 자동 Idle 복귀
        if (attackStack >= 4)
        {
            yield return new WaitForSeconds(0.3f);
            ResetStackAndReturnIdle();
            yield break;
        }

        // 1~3 스택 상태에서 추가 입력 대기
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
        {
            ResetStackAndReturnIdle();
        }
        else
        {
            StartCoroutine(CoStackAttack());
        }
    }

    private void ResetStackAndReturnIdle()
    {
        attackStack = 0;
        isAttacking = false;
        currentState = PlayerState.Idle;
        canStartupCancel = true; // 선딜 캔슬 플래그 초기화
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
        {
            playerAttackZone.EnableAttackCollider(true);
        }
        transform.Translate(Vector3.forward * 1.0f, Space.Self);
    }

    public void OnAttack1DamageEnd()
    {
        Debug.Log("[Attack_1] 공격 판정 종료");
        if (playerAttackZone != null)
        {
            playerAttackZone.EnableAttackCollider(false);
        }
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
        {
            ResetStackAndReturnIdle();
        }
    }
    #endregion

    #region 피격 (Hit) 처리
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (isDead || currentState == PlayerState.Death)
            return;

        if (!isHit)
        {
            StartCoroutine(CoHitReaction());
        }
    }

    private IEnumerator CoHitReaction()
    {
        isHit = true;
        currentState = PlayerState.Hit;

        if (animator != null)
        {
            animator.SetBool("isHit", true);
        }

        yield return new WaitForSeconds(0.5f);

        isHit = false;
        if (animator != null)
        {
            animator.SetBool("isHit", false);
        }

        if (!isDead)
        {
            currentState = PlayerState.Idle;
        }
    }
    #endregion

    #region 마우스 방향 바라보기
    
    // 마우스 위치를 기준으로 캐릭터가 해당 방향을 바라보도록 회전
    
    private void FaceMouseDirection()
    {
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
            {
                transform.forward = lookDir.normalized;
            }
        }
    }
    #endregion
}
