using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
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
    [Header("Dash Double Click 설정")]
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

    [Header("UI 관련")]
    public GameObject pauseMenuPanel;
    public Button quitButton;
    public Button lobbyButton;

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
    // (기존 키 설정은 더 이상 사용하지 않습니다)

    // 공격 쿨타임 체크용 변수
    private float lastBasicAttackTime = -Mathf.Infinity;
    private float lastSpecialAttackTime = -Mathf.Infinity;
    private float lastSkillTime = -Mathf.Infinity;
    private float lastUltimateTime = -Mathf.Infinity;

    // 평타 스택 변수
    private int basicAttackStack = 0;
    private float lastBasicAttackStackTime = 0f;

    // Animator 참조
    private Animator animator;

    // 공격 진행 여부
    private bool isAttacking = false;

    // 플레이어 상태
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Death }
    private PlayerState currentState = PlayerState.Idle;

    // Trap 및 NPC 관련 변수
    private int trapClearCount = 0;
    private GameObject currentTrap = null;
    private bool isTrapCleared = false;
    public GameObject dialoguePanel;
    public UnityEngine.UI.Text dialogueText;
    public string[] npcDialogues;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;

    // === 새 인풋 시스템 관련 변수 ===
    private PlayerInputActions inputActions; // 자동 생성된 Input Action 클래스

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

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButton);
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(OnLobbyButton);

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        basicAttackStack = 0;
        lastBasicAttackStackTime = Time.time;
        animator.SetInteger("AttackStack", basicAttackStack);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentState = PlayerState.Idle;
        animator.SetBool("isRunning", false);

        currentHealth = maxHealth;

        if (damageCollider != null)
        {
            damageCollider.radius = damageColliderRadius;
        }
    }

    void Update()
    {
        // UI나 죽은 상태에서는 입력 무시
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            return;
        if (currentState == PlayerState.Death)
            return;

        // === 대쉬 입력 처리 (기존 Space키 → 새 액션 "Dash") ===
        if (inputActions.Player.Dash.triggered)
        {
            if (Time.time - lastDashClickTime <= dashDoubleClickThreshold)
            {
                // 이동 액션에서 이동값 읽기 (Vector2 → Vector3)
                Vector2 dashInput = inputActions.Player.Move.ReadValue<Vector2>();
                Vector3 dashDirection = new Vector3(dashInput.x, 0, dashInput.y);
                if (dashDirection == Vector3.zero)
                    dashDirection = transform.forward;
                StartCoroutine(DoDash(dashDirection));
                lastDashClickTime = -Mathf.Infinity;
                return;
            }
            else
            {
                lastDashClickTime = Time.time;
            }
        }

        if (Time.time - lastBasicAttackStackTime >= basicAttackResetTime)
        {
            basicAttackStack = 0;
            animator.SetInteger("AttackStack", basicAttackStack);
        }

        // === 일시정지 입력 처리 (새 액션 "Pause") ===
        if (inputActions.Player.Pause.triggered)
        {
            TogglePauseMenu();
            return;
        }

        // === 공격 입력 처리 (각 액션의 triggered 상태 사용) ===
        bool basicAttackInput = inputActions.Player.BasicAttack.triggered;
        bool specialAttackInput = inputActions.Player.SpecialAttack.triggered;
        bool skillAttackInput = inputActions.Player.SkillAttack.triggered;
        bool ultimateAttackInput = inputActions.Player.UltimateAttack.triggered;

        if (basicAttackInput || specialAttackInput || skillAttackInput || ultimateAttackInput)
        {
            HandleActions();
        }
        else
        {
            // === 이동 입력 처리 (새 액션 "Move") ===
            Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
            {
                basicAttackStack = 0;
                animator.SetInteger("AttackStack", basicAttackStack);
                currentState = PlayerState.Run;
                HandleMovement();
            }
            else
            {
                currentState = PlayerState.Idle;
                animator.SetBool("isRunning", false);
            }
        }

        if (centerPoint != null)
            centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;

        if (currentState != PlayerState.Attack_L && currentState != PlayerState.Attack_R &&
            currentState != PlayerState.Skill && currentState != PlayerState.Ultimate)
        {
            CheckInteractions();
        }
    }

    void HandleMovement()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float h = moveInput.x;
        float v = moveInput.y;
        Vector3 inputDir = new Vector3(h, 0, v);

        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = inputDir.normalized;
            float moveSpeed = (Mathf.Abs(h) > 0 && Mathf.Approximately(v, 0f)) ? speedHorizontal : speedVertical;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.z);
            animator.SetBool("isRunning", true);

            if (centerPoint != null)
                centerPoint.position = transform.position + moveDir * centerPointOffsetDistance;
            UpdateCenterPoints(moveDir);
        }
        else
        {
            animator.SetBool("isRunning", false);
            currentState = PlayerState.Idle;
            if (centerPoint != null)
                centerPoint.position = transform.position + transform.forward * centerPointOffsetDistance;
        }
    }

    void UpdateCenterPoints(Vector3 moveDir)
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
            else
            {
                Debug.LogWarning($"centerPoints[{i}] 미할당");
            }
        }
    }

    IEnumerator DoDash(Vector3 direction)
    {
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
        if (inputActions.Player.Move.ReadValue<Vector2>().magnitude > 0.1f)
        {
            currentState = PlayerState.Run;
            animator.SetBool("isRunning", true);
        }
        else
        {
            currentState = PlayerState.Idle;
            animator.SetBool("isRunning", false);
        }
    }

    void HandleActions()
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
                animator.SetInteger("AttackStack", basicAttackStack);
                StartCoroutine(PerformBasicAttack());
            }
        }
        else if (inputActions.Player.SpecialAttack.triggered)
        {
            if (Time.time - lastSpecialAttackTime >= specialAttackCooldown)
            {
                lastSpecialAttackTime = Time.time;
                StartCoroutine(PerformSpecialAttack());
            }
        }
        else if (inputActions.Player.SkillAttack.triggered)
        {
            if (Time.time - lastSkillTime >= skillCooldown)
            {
                lastSkillTime = Time.time;
                PerformSkillAttack();
            }
        }
        else if (inputActions.Player.UltimateAttack.triggered)
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
        animator.SetBool("isAttacking", true);
        animator.SetTrigger("Attack_L");
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
                    animator.SetInteger("AttackStack", basicAttackStack);
                    yield return StartCoroutine(PerformBasicAttack());
                    yield break;
                }
                waitTimer += Time.deltaTime;
                yield return null;
            }
            basicAttackStack = 0;
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
        animator.SetBool("isAttacking", false);
    }

    IEnumerator PerformSpecialAttack()
    {
        currentState = PlayerState.Attack_R;
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
    }

    void PerformSkillAttack()
    {
        currentState = PlayerState.Skill;
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
    }

    void PerformUltimateAttack()
    {
        currentState = PlayerState.Ultimate;
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
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        currentState = (moveInput.magnitude > 0.1f) ? PlayerState.Run : PlayerState.Idle;
    }

    void CheckInteractions()
    {
        if (isDialogueActive)
        {
            if (inputActions.Player.NPCInteract.triggered)
            {
                currentDialogueIndex++;
                if (currentDialogueIndex < npcDialogues.Length)
                    dialogueText.text = npcDialogues[currentDialogueIndex];
                else
                {
                    dialoguePanel.SetActive(false);
                    isDialogueActive = false;
                    currentDialogueIndex = 0;
                }
            }
            return;
        }

        if (inputActions.Player.StageProgress.triggered)
        {
            if (!isTrapCleared)
                Debug.Log("조건을 충족시켜야 다음으로 넘어갑니다.");
            else
                Debug.Log("다음 스테이지로 넘어갑니다.");
        }

        Collider[] cols = Physics.OverlapSphere(centerPoint.position, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
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
            else if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                if (inputActions.Player.NPCInteract.triggered)
                {
                    isDialogueActive = true;
                    currentDialogueIndex = 0;
                    dialoguePanel.SetActive(true);
                    if (npcDialogues != null && npcDialogues.Length > 0)
                        dialogueText.text = npcDialogues[currentDialogueIndex];
                }
            }
        }
    }

    void TogglePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            bool isActive = pauseMenuPanel.activeSelf;
            pauseMenuPanel.SetActive(!isActive);
        }
    }

    void OnQuitButton() { TogglePauseMenu(); }
    void OnLobbyButton() { Debug.Log("로비로 이동 (추후 구현)"); }

    public void Die()
    {
        currentState = PlayerState.Death;
        animator.SetTrigger("Death");
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
