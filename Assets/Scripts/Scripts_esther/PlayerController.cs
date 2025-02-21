using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("이동 속도 설정 (8방향)")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;
    public float speedDiagonalLeftUp = 4.5f;
    public float speedDiagonalRightUp = 4.5f;
    public float speedDiagonalLeftDown = 4.5f;
    public float speedDiagonalRightDown = 4.5f;

    [Header("대쉬 설정")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    private float lastDashClickTime = -Mathf.Infinity;

    [Header("공격 및 스킬 데미지 설정")]
    public int basicAttackDamage = 10;
    public int specialAttackDamage = 15;
    public int skillDamage = 30;
    public int ultimateDamage = 50;

    [Header("공격 범위 설정")]
    public float basicAttackRange = 1.5f;
    public float specialAttackRange = 1.5f;
    public float skillRange = 2f;
    public float ultimateRange = 3f;

    [Header("쿨타임 설정 (초)")]
    public float basicAttackCooldown = 0.5f;
    public float specialAttackCooldown = 1f;
    public float skillCooldown = 2f;
    public float ultimateCooldown = 3f;

    [Header("평타 스택 초기화 (2초)")]
    public float basicAttackResetTime = 2f;

    [Header("중심점 설정")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("중심점 설정 (8개)")]
    [SerializeField] private Transform[] centerPoints = new Transform[8];

    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("플레이어 데미지 범위 설정")]
    public SphereCollider damageCollider;
    public float damageColliderRadius = 0.7f;

    [Header("상호작용 설정")]
    public LayerMask interactionLayerMask;
    public float interactionRadius = 1.5f;

    // 함정 관련 변수
    private int trapClearCount = 0;
    private GameObject currentTrap = null;
    private bool isTrapCleared = false;

    // 공격 쿨타임 체크용
    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    // 평타 스택
    private int basicAttackStack = 0;
    private float lastBasicAttackStackTime = 0f;

    // Animator
    private Animator animator;

    // 공격 진행 여부
    private bool isAttacking = false;

    // 플레이어 상태
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    private PlayerState currentState = PlayerState.Idle;

    // 인풋 시스템
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayerController] Animator 컴포넌트가 없습니다!");
        }

        // Photon 소유권 체크
        if (!photonView.IsMine)
        {
          
            this.enabled = false;
            return;
        }

        // 초기화
        basicAttackStack = 0;
        lastBasicAttackStackTime = Time.time;
        if (animator != null)
        {
            animator.SetInteger("AttackStack", basicAttackStack);
            animator.SetBool("isRunning", false);
        }

        currentState = PlayerState.Idle;
        currentHealth = maxHealth;

        if (damageCollider != null)
        {
            damageCollider.radius = damageColliderRadius;
        }

        if (centerPoint != null)
        {
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }
    }

    void Update()
    {
        // UIManager_player가 존재하고, 일시정지 패널이 열려 있으면 입력 무시
        if (UIManager_player.Instance != null && UIManager_player.Instance.pauseMenuPanel != null)
        {
            if (UIManager_player.Instance.pauseMenuPanel.activeSelf)
                return;
        }
        if (currentState == PlayerState.Death) return;

        // 대쉬 처리
        if (inputActions.Player.Dash.triggered)
        {
            if (Time.time - lastDashClickTime <= dashDoubleClickThreshold)
            {
                Vector2 dashInput = inputActions.Player.Move.ReadValue<Vector2>();
                Vector3 dashDir = new Vector3(dashInput.x, 0, dashInput.y);
                if (dashDir == Vector3.zero)
                    dashDir = transform.forward;

                StartCoroutine(DoDash(dashDir));
                lastDashClickTime = -Mathf.Infinity;
                return;
            }
            else
            {
                lastDashClickTime = Time.time;
            }
        }

        // 평타 스택 초기화
        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            basicAttackStack = 0;
            if (animator != null)
                animator.SetInteger("AttackStack", basicAttackStack);
        }

        // 일시정지 입력 처리
        if (inputActions.Player.Pause.triggered)
        {
            UIManager_player.Instance?.TogglePauseMenu();
            return;
        }

        // 공격/스킬 입력
        bool basicAttackInput = inputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = inputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = inputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = inputActions.Player.UltimateAttack.triggered;
        if (basicAttackInput || specialAttackInput || skillAttackInput || ultimateAttackInput)
        {
            HandleActions();
        }

        // 이동
        HandleMovement();

        // 중심점 업데이트
        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        // 상호작용
        if (currentState != PlayerState.Attack_L && currentState != PlayerState.Attack_R &&
            currentState != PlayerState.Skill && currentState != PlayerState.Ultimate)
        {
            CheckInteractions();
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float h = moveInput.x;
        float v = moveInput.y;

        bool isMoving = (Mathf.Abs(h) > 0f || Mathf.Abs(v) > 0f);
        bool canMoveState = (currentState != PlayerState.Attack_L &&
                             currentState != PlayerState.Attack_R &&
                             currentState != PlayerState.Skill &&
                             currentState != PlayerState.Ultimate &&
                             currentState != PlayerState.Death);

        if (isMoving)
        {
            if (canMoveState)
            {
                currentState = PlayerState.Run;
                if (animator != null)
                    animator.SetBool("isRunning", true);
            }

            Vector3 moveDir = new Vector3(h, 0, v).normalized;
            float moveSpeed = (Mathf.Abs(h) > 0 && Mathf.Approximately(v, 0f))
                ? speedHorizontal : speedVertical;

            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

            if (animator != null)
            {
                animator.SetFloat("moveX", moveDir.x);
                animator.SetFloat("moveY", moveDir.z);
            }

            UpdateCenterPoints(moveDir);
        }
        else
        {
            if (canMoveState)
            {
                currentState = PlayerState.Idle;
                if (animator != null)
                    animator.SetBool("isRunning", false);
            }
        }
    }

    private void UpdateCenterPoints(Vector3 moveDir)
    {
        if (centerPoints == null || centerPoints.Length != 8)
        {
            Debug.LogWarning("centerPoints 배열 8개를 Inspector에서 할당하세요.");
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            if (centerPoints[i] != null)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0, angle, 0) * moveDir * centerPointOffsetDistance;
                centerPoints[i].position = transform.position + offset;
            }
        }
    }

    IEnumerator DoDash(Vector3 direction)
    {
        if (animator != null)
            animator.SetTrigger("Dash");

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * dashDistance;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput.magnitude > 0.1f)
        {
            currentState = PlayerState.Run;
            if (animator != null)
                animator.SetBool("isRunning", true);
        }
        else
        {
            currentState = PlayerState.Idle;
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }

    private void HandleActions()
    {
        if (inputActions.Player.BasicAttack.triggered && !isAttacking)
        {
            if (Time.time - lastBasicAttackTime >= basicAttackCooldown)
            {
                if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
                    basicAttackStack = 1;
                else if (basicAttackStack < 4)
                    basicAttackStack++;

                lastBasicAttackTime = Time.time;
                lastBasicAttackStackTime = Time.time;
                if (animator != null)
                    animator.SetInteger("AttackStack", basicAttackStack);

                StartCoroutine(PerformBasicAttack());
            }
        }
        else if (inputActions.Player.SpecialAttack.triggered && !isAttacking)
        {
            if (Time.time - lastSpecialAttackTime >= specialAttackCooldown)
            {
                lastSpecialAttackTime = Time.time;
                StartCoroutine(PerformSpecialAttack());
            }
        }
        else if (inputActions.Player.SkillAttack.triggered && !isAttacking)
        {
            if (Time.time - lastSkillTime >= skillCooldown)
            {
                lastSkillTime = Time.time;
                PerformSkillAttack();
            }
        }
        else if (inputActions.Player.UltimateAttack.triggered && !isAttacking)
        {
            if (Time.time - lastUltimateTime >= ultimateCooldown)
            {
                lastUltimateTime = Time.time;
                PerformUltimateAttack();
            }
        }
    }

    IEnumerator PerformBasicAttack()
    {
        isAttacking = true;
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            animator.SetTrigger("Attack_L");
        }
        currentState = PlayerState.Attack_L;
        yield return new WaitForSeconds(0.2f);

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, basicAttackRange, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            Collider closest = cols[0];
            float minDist = Vector3.Distance(checkPos, closest.transform.position);
            foreach (Collider col in cols)
            {
                float dist = Vector3.Distance(checkPos, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col;
                }
            }
            EnemyController enemy = closest.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(basicAttackDamage);
                Debug.Log($"[평타] 스택 {basicAttackStack}: 적에게 {basicAttackDamage} 데미지. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }

        if (basicAttackStack >= 4)
        {
            float waitTimer = 0f;
            while (waitTimer < basicAttackResetTime)
            {
                if (inputActions.Player.BasicAttack.triggered)
                {
                    lastBasicAttackTime = Time.time;
                    lastBasicAttackStackTime = Time.time;
                    if (animator != null)
                        animator.SetInteger("AttackStack", basicAttackStack);
                    yield return StartCoroutine(PerformBasicAttack());
                    yield break;
                }
                waitTimer += Time.deltaTime;
                yield return null;
            }
            basicAttackStack = 0;
            if (animator != null)
                animator.SetInteger("AttackStack", basicAttackStack);
            currentState = PlayerState.Idle;
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        }

        isAttacking = false;
        if (animator != null)
            animator.SetBool("isAttacking", false);
    }

    IEnumerator PerformSpecialAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Attack_R;
        if (animator != null)
            animator.SetTrigger("Attack_R");
        yield return new WaitForSeconds(0.2f);

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, specialAttackRange, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            Collider closest = cols[0];
            float minDist = Vector3.Distance(checkPos, closest.transform.position);
            foreach (Collider col in cols)
            {
                float dist = Vector3.Distance(checkPos, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col;
                }
            }
            EnemyController enemy = closest.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(specialAttackDamage);
                Debug.Log($"[특수] 적에게 {specialAttackDamage} 데미지. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }

        yield return new WaitForSeconds(0.3f);
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        isAttacking = false;
    }

    private void PerformSkillAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Skill;
        if (animator != null)
            animator.SetTrigger("Skill");

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, skillRange, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            Collider closest = cols[0];
            float minDist = Vector3.Distance(checkPos, closest.transform.position);
            foreach (Collider col in cols)
            {
                float dist = Vector3.Distance(checkPos, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col;
                }
            }
            EnemyController enemy = closest.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(skillDamage);
                Debug.Log($"[스킬] 적에게 {skillDamage} 데미지. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        StartCoroutine(TransitionAfterSkill());
    }

    IEnumerator TransitionAfterSkill()
    {
        yield return new WaitForSeconds(0.3f);
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        isAttacking = false;
    }

    private void PerformUltimateAttack()
    {
        isAttacking = true;
        currentState = PlayerState.Ultimate;
        if (animator != null)
            animator.SetTrigger("Ultimate");

        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, ultimateRange, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            Collider closest = cols[0];
            float minDist = Vector3.Distance(checkPos, closest.transform.position);
            foreach (Collider col in cols)
            {
                float dist = Vector3.Distance(checkPos, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col;
                }
            }
            EnemyController enemy = closest.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(ultimateDamage);
                Debug.Log($"[궁극] 적에게 {ultimateDamage} 데미지. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        StartCoroutine(TransitionAfterUltimate());
    }

    IEnumerator TransitionAfterUltimate()
    {
        yield return new WaitForSeconds(0.3f);
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
        isAttacking = false;
    }

    private void CheckInteractions()
    {
        // 대화 중이면, UIManager_player를 통해 대화 진행
        if (UIManager_player.Instance != null && UIManager_player.Instance.IsDialogueActive())
        {
            if (inputActions.Player.NPCInteract.triggered)
            {
                UIManager_player.Instance.NextDialogue();
            }
            return;
        }

        // 상호작용(함정/대화) 범위 확인
        Vector3 checkPos = (centerPoint != null) ? centerPoint.position : transform.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            // 함정
            if (col.gameObject.layer == LayerMask.NameToLayer("Trap"))
            {
                currentTrap = col.gameObject;
                if (inputActions.Player.TrapClear.triggered)
                {
                    trapClearCount++;
                    Debug.Log("함정 키 입력 횟수: " + trapClearCount);
                    if (trapClearCount >= 2)
                    {
                        isTrapCleared = true;
                        Debug.Log("함정 해제됨.");
                        Destroy(currentTrap);
                        trapClearCount = 0;
                        currentTrap = null;
                    }
                }
            }
            // NPC
            else if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                if (inputActions.Player.NPCInteract.triggered)
                {
                    UIManager_player.Instance?.StartDialogue();
                }
            }
        }
    }

    public void Die()
    {
        currentState = PlayerState.Death;
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        else
        {
            Debug.LogWarning("[PlayerController] Animator가 null이라 Death 애니메이션을 실행할 수 없습니다.");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("플레이어 체력: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
}
