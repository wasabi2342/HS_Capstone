using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float horizontalSpeed = 5f;
    public float verticalSpeed = 5f;
    public float diagonalSpeed = 4.5f;

    [Header("공격 및 스킬 데미지 설정")]
    public int basicAttackDamage = 10;       // 평타 데미지 (마우스 좌클릭)
    public int specialAttackDamage = 15;     // 특수 공격 데미지 (마우스 우클릭)
    public int skillDamage = 30;             // 스킬 데미지 (좌 Shift)
    public int ultimateDamage = 50;          // 궁극기 데미지 (R키)

    [Header("공격 범위 설정")]
    public float basicAttackRange = 1.5f;      // 평타 공격 범위 (예: 1.5 유닛)
    public float specialAttackRange = 1.5f;    // 특수 공격 범위 (예: 1.5 유닛)
    public float skillRange = 2f;              // 스킬 공격 범위 (예: 2 유닛)
    public float ultimateRange = 3f;           // 궁극기 공격 범위 (예: 3 유닛)

    [Header("UI 관련")]
    public GameObject pauseMenuPanel;
    public Button quitButton;
    public Button lobbyButton;

    [Header("무기 관련")]
    [Tooltip("플레이어 자식에 배치된 Weapon 오브젝트의 Weapon 스크립트를 할당합니다.")]
    public Weapon weapon;  // (현재 평타/특수 공격에는 사용되지 않습니다.)

    private Animator animator;
    private enum PlayerState { Idle, Moving, Attacking, UsingSkill, UsingUltimate, Dashing, InUI }
    private PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButton);
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(OnLobbyButton);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
            return;
        }

        if (currentState == PlayerState.InUI)
            return;

        HandleMovement();
        HandleActions();
    }

    // 플레이어 이동 처리 (카메라 기준)
    void HandleMovement()
    {
        if (currentState == PlayerState.Attacking || currentState == PlayerState.UsingSkill ||
            currentState == PlayerState.UsingUltimate || currentState == PlayerState.Dashing)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v);

        if (inputDir != Vector3.zero)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            Vector3 moveDir = (camForward * v) + (camRight * h);
            moveDir.Normalize();

            float speed = horizontalSpeed;
            if (Mathf.Abs(h) > 0 && Mathf.Abs(v) > 0)
                speed = diagonalSpeed;
            else if (Mathf.Abs(v) > 0)
                speed = verticalSpeed;

            transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.z);
            animator.SetBool("isMoving", true);
            currentState = PlayerState.Moving;
        }
        else
        {
            animator.SetBool("isMoving", false);
            currentState = PlayerState.Idle;
        }

        if (Input.GetKeyDown(KeyCode.Space) && inputDir != Vector3.zero)
        {
            StartCoroutine(DoDash(inputDir));
        }
    }

    // 공격 입력 처리
    void HandleActions()
    {
        // 평타 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(UseBasicAttack());
        }
        // 특수 공격 (마우스 우클릭)
        else if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(UseSpecialAttack());
        }
        // 스킬 공격 (좌 Shift)
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            UseSkillAttack();
        }
        // 궁극기 (R키)
        else if (Input.GetKeyDown(KeyCode.R))
        {
            UseUltimateAttack();
        }
    }

    // 평타: 지정한 범위 내에 있는 적 중 가장 가까운 적에게 데미지 적용
    IEnumerator UseBasicAttack()
    {
        currentState = PlayerState.Attacking;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.2f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, basicAttackRange, LayerMask.GetMask("Enemy"));
        if (hitColliders.Length > 0)
        {
            Collider closest = hitColliders[0];
            float minDist = Vector3.Distance(transform.position, closest.transform.position);
            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
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
                Debug.Log($"[평타] 적에게 {basicAttackDamage}의 데미지를 입혔습니다. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        yield return new WaitForSeconds(0.3f);
        currentState = PlayerState.Idle;
    }

    // 특수 공격: 지정한 범위 내에 있는 적 중 가장 가까운 적에게 데미지 적용
    IEnumerator UseSpecialAttack()
    {
        currentState = PlayerState.Attacking;
        animator.SetTrigger("SpecialAttack");
        yield return new WaitForSeconds(0.2f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, specialAttackRange, LayerMask.GetMask("Enemy"));
        if (hitColliders.Length > 0)
        {
            Collider closest = hitColliders[0];
            float minDist = Vector3.Distance(transform.position, closest.transform.position);
            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
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
                Debug.Log($"[특수 공격] 적에게 {specialAttackDamage}의 데미지를 입혔습니다. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        yield return new WaitForSeconds(0.3f);
        currentState = PlayerState.Idle;
    }

    // 스킬 공격: 플레이어 주변 skillRange 내의 적 중 가장 가까운 적에게 데미지 적용
    void UseSkillAttack()
    {
        currentState = PlayerState.UsingSkill;
        animator.SetTrigger("Skill");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, skillRange, LayerMask.GetMask("Enemy"));
        if (hitColliders.Length > 0)
        {
            Collider closest = hitColliders[0];
            float minDist = Vector3.Distance(transform.position, closest.transform.position);
            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
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
                Debug.Log($"[스킬] 적에게 {skillDamage}의 데미지를 입혔습니다. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        currentState = PlayerState.Idle;
    }

    // 궁극기 공격: 플레이어 주변 ultimateRange 내의 적 중 가장 가까운 적에게 데미지 적용
    void UseUltimateAttack()
    {
        currentState = PlayerState.UsingUltimate;
        animator.SetTrigger("Ultimate");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRange, LayerMask.GetMask("Enemy"));
        if (hitColliders.Length > 0)
        {
            Collider closest = hitColliders[0];
            float minDist = Vector3.Distance(transform.position, closest.transform.position);
            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
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
                Debug.Log($"[궁극기] 적에게 {ultimateDamage}의 데미지를 입혔습니다. 남은 체력: {enemy.GetCurrentHealth()}");
            }
        }
        currentState = PlayerState.Idle;
    }

    // 돌진 (Dash) 처리
    IEnumerator DoDash(Vector3 direction)
    {
        currentState = PlayerState.Dashing;
        animator.SetTrigger("Dash");

        float dashDistance = 2f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + direction.normalized * dashDistance;
        float dashDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        currentState = PlayerState.Idle;
    }

    // 메뉴 토글 (ESC 키)
    void TogglePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            bool isActive = pauseMenuPanel.activeSelf;
            pauseMenuPanel.SetActive(!isActive);
            currentState = (!isActive) ? PlayerState.InUI : PlayerState.Idle;
        }
    }

    void OnQuitButton()
    {
        TogglePauseMenu();
    }

    void OnLobbyButton()
    {
        Debug.Log("로비로 이동 (추후 구현)");
        // SceneManager.LoadScene("LobbyScene");
    }
}
