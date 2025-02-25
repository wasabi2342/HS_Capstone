using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// BasePlayerController를 상속.
/// PlayerAttackZone을 통해 범위 내 적(enemiesInRange)에게 데미지.
/// AttackStack (int) 파라미터로 스택 모션 구현.
/// </summary>
public class PlayerController : BasePlayerController
{
    // 로컬 플레이어 정적 참조
    public static PlayerController localPlayer;

    [Header("8방향 이동 속도 설정")]
    public float moveSpeedHorizontal = 5f;
    public float moveSpeedVertical = 5f;
    public float moveSpeedDiagonalLeftUp = 4.5f;
    public float moveSpeedDiagonalRightUp = 4.5f;
    public float moveSpeedDiagonalLeftDown = 4.5f;
    public float moveSpeedDiagonalRightDown = 4.5f;

    [Header("중심점 설정 (8개)")]
    [SerializeField] private Transform[] centerPoints = new Transform[8];

    [Header("스택 모션 설정")]
    public float comboInputTime = 2f; // 2초 이내에 추가 입력 없으면 Idle
    private int attackStack = 0;      // 0=idle, 1=Attack1, 2=Attack2, 3=Attack3, 4=Attack4

    // 공격 쿨타임
    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    // PlayerAttackZone 참조
    private PlayerAttackZone playerAttackZone;

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
        base.Update(); // 부모 로직(이동/대쉬 등)
        if (!photonView.IsMine) return;
        if (isDead || currentState == PlayerState.Death) return;

        HandleActions();
        HandleMovement();

        // Animator 파라미터: AttackStack
        if (animator != null)
        {
            // AttackStack (int) 파라미터로 스택 모션 전환
            animator.SetInteger("AttackStack", attackStack);
            animator.SetBool("isAttacking", isAttacking);
        }
    }

    #region 공격 & 스킬 (스택 모션)

    protected override void HandleActions()
    {
        if (isAttacking) return;

        bool basicAttackInput = playerInputActions.Player.BasicAttack.triggered;   // 마우스 좌클
        bool specialAttackInput = playerInputActions.Player.SpecialAttack.triggered; // 마우스 우클
        bool skillAttackInput = playerInputActions.Player.SkillAttack.triggered;   // 왼Shift
        bool ultimateAttackInput = playerInputActions.Player.UltimateAttack.triggered;// R키

        // 평타 (스택 모션)
        if (basicAttackInput && Time.time - lastBasicAttackTime >= 0.5f)
        {
            StartCoroutine(CoStackAttack());
        }
        else if (specialAttackInput && Time.time - lastSpecialAttackTime >= 1f)
        {
            // ...
        }
        else if (skillAttackInput && Time.time - lastSkillTime >= 2f)
        {
            // ...
        }
        else if (ultimateAttackInput && Time.time - lastUltimateTime >= 3f)
        {
            // ...
        }
    }

    private IEnumerator CoStackAttack()
    {
        attackStack++;
        if (attackStack > 4) attackStack = 4;

        isAttacking = true;

        // 공격 스택별 PlayerState
        switch (attackStack)
        {
            case 1: currentState = PlayerState.Attack_L; break;   // Attack_1
            case 2: currentState = PlayerState.Attack_R; break;   // Attack_2
            case 3: currentState = PlayerState.Skill; break;   // Attack_3
            case 4: currentState = PlayerState.Ultimate; break;   // Attack_4
        }

        yield return new WaitForSeconds(0.2f);

        // 범위 내 적에게 데미지
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

        // 스택=4이면 자동 Idle
        if (attackStack >= 4)
        {
            yield return new WaitForSeconds(0.3f);
            ResetStackAndReturnIdle();
            yield break;
        }

        // AttackStack=1~3
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
        }
    }

    #endregion

    #region 8방향 이동 (부모와 동일 or override)

    // HandleMovement(), etc.는 Base에서 override

    #endregion
}
