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
    private int attackStack = 0;      // 0: idle, 1: Attack1, 2: Attack2, 3: Attack3, 4: Attack4

    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    // PlayerAttackZone 참조 (근접 공격 범위)
    private PlayerAttackZone playerAttackZone;

    // 지면을 Raycast할 때 사용할 레이어 (필요 시 설정-> 아직 보류/ 테스트할 때, 사용예정)
    //public LayerMask groundLayer;

    private PlayerInputActions playerInputActions;

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

        playerAttackZone = GetComponentInChildren<PlayerAttackZone>();
        if (playerAttackZone == null)
        {
            Debug.LogWarning("[PlayerController] PlayerAttackZone이 자식 오브젝트에 없습니다!");
        }

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
        base.Update(); // 부모가 이동, 대쉬, CenterPoint 갱신, 상호작용 메서드를 처리
        if (!photonView.IsMine) return;
        if (isDead || currentState == PlayerState.Death) return;

        // 공격/스킬 입력 처리
        HandleActions();

        // 애니메이터 파라미터 세팅
        if (animator != null)
        {
            animator.SetInteger("AttackStack", attackStack);
            animator.SetBool("isAttacking", isAttacking);
        }
    }

    #region 8방향 이동 Override
    protected override void HandleMovement()
    {
        if (currentState == PlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            // 부모에서 이미 currentDirectionIndex, centerPoint 갱신을 처리
            // 여기서는 이동만 수행
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

    #region 공격 & 스킬 (스택 모션)
    protected override void HandleActions()
    {
        if (isAttacking) return;

        bool basicAttackInput = playerInputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = playerInputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = playerInputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = playerInputActions.Player.UltimateAttack.triggered;

        // 마우스 좌클릭 공격
        if (basicAttackInput && Time.time - lastBasicAttackTime >= 0.5f)
        {
            lastBasicAttackTime = Time.time;

            // 공격하기 직전에 마우스 방향으로 캐릭터가 바라보도록 설정
            FaceMouseDirection();

            StartCoroutine(CoStackAttack());
        }
        else if (specialAttackInput && Time.time - lastSpecialAttackTime >= 1f)
        {
            lastSpecialAttackTime = Time.time;
            // 스페셜 공격 시도 시에도 마찬가지로 마우스 방향 바라보게 가능
            FaceMouseDirection();
            
        }
        else if (skillAttackInput && Time.time - lastSkillTime >= 2f)
        {
            lastSkillTime = Time.time;
            FaceMouseDirection();
            
        }
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= 3f)
        {
            lastUltimateTime = Time.time;
            FaceMouseDirection();
           
        }
    }

   
    // 카메라 메인 기준으로 마우스 위치 → 월드 좌표로 변환 후, 플레이어가 그 방향을 바라보게 함
   
    private void FaceMouseDirection()
    {
        // 1. 마우스 화면 좌표 획득
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 2. 카메라에서 마우스 좌표로 Ray 발사
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));

        // 3. y=0 평면(지면)과의 교차점을 구함 (탑다운/쿼터뷰)
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        

        if (plane.Raycast(ray, out float distance))
        {
            // 4. Ray와 평면이 교차하는 지점(월드 좌표)
            Vector3 hitPoint = ray.GetPoint(distance);

            // 5. 플레이어가 바라볼 방향(수평 회전만)
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f; // 고정

            if (lookDir.sqrMagnitude > 0.001f)
            {
                // 6. 해당 방향으로 회전
                transform.forward = lookDir.normalized;
            }
        }
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

        // 공격 모션 재생 잠깐 대기
        yield return new WaitForSeconds(0.2f);

        // 근접 공격 범위 내 적에게 데미지
        if (playerAttackZone != null)
        {
            int damage = (attackStack == 1) ? 10 :
                         (attackStack == 2) ? 15 :
                         (attackStack == 3) ? 30 : 50; // 4

            foreach (GameObject enemyObj in playerAttackZone.enemiesInRange)
            {
                var enemy = enemyObj.GetComponentInParent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[PlayerController] 공격: {damage} 데미지 → 남은 체력: {enemy.GetCurrentHealth()}");
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

        // AttackStack=1~3일 때 추가 입력 대기
        float timer = 0f;
        bool nextAttack = false;
        while (timer < comboInputTime)
        {
            timer += Time.deltaTime;
            // 이동 입력 발생 시 -> Idle 복귀
            if (moveInput.magnitude > 0.01f || isDashing)
            {
                ResetStackAndReturnIdle();
                yield break;
            }
            // 추가 평타 입력
            if (playerInputActions.Player.BasicAttack.triggered)
            {
                // 공격 전에도 다시 마우스 방향 보도록
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
            // 다음 스택 공격
            StartCoroutine(CoStackAttack());
        }
    }

    private void ResetStackAndReturnIdle()
    {
        attackStack = 0;
        isAttacking = false;
        currentState = PlayerState.Idle;
        if (animator != null)
        {
            animator.SetInteger("AttackStack", 0);
            animator.SetBool("isAttacking", false);
        }
    }
    #endregion

    
}
