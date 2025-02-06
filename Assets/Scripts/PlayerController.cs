using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("�̵� �ӵ� ����")]
    public float horizontalSpeed = 5f;
    public float verticalSpeed = 5f;
    public float diagonalSpeed = 4.5f;

    [Header("���� �� ��ų ������ ����")]
    public int basicAttackDamage = 10;       // ��Ÿ ������ (���콺 ��Ŭ��)
    public int specialAttackDamage = 15;     // Ư�� ���� ������ (���콺 ��Ŭ��)
    public int skillDamage = 30;             // ��ų ������ (�� Shift)
    public int ultimateDamage = 50;          // �ñر� ������ (RŰ)

    [Header("���� ���� ����")]
    public float basicAttackRange = 1.5f;      // ��Ÿ ���� ���� (��: 1.5 ����)
    public float specialAttackRange = 1.5f;    // Ư�� ���� ���� (��: 1.5 ����)
    public float skillRange = 2f;              // ��ų ���� ���� (��: 2 ����)
    public float ultimateRange = 3f;           // �ñر� ���� ���� (��: 3 ����)

    [Header("UI ����")]
    public GameObject pauseMenuPanel;
    public Button quitButton;
    public Button lobbyButton;

    [Header("���� ����")]
    [Tooltip("�÷��̾� �ڽĿ� ��ġ�� Weapon ������Ʈ�� Weapon ��ũ��Ʈ�� �Ҵ��մϴ�.")]
    public Weapon weapon;  // (���� ��Ÿ/Ư�� ���ݿ��� ������ �ʽ��ϴ�.)

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

    // �÷��̾� �̵� ó�� (ī�޶� ����)
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

    // ���� �Է� ó��
    void HandleActions()
    {
        // ��Ÿ (���콺 ��Ŭ��)
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(UseBasicAttack());
        }
        // Ư�� ���� (���콺 ��Ŭ��)
        else if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(UseSpecialAttack());
        }
        // ��ų ���� (�� Shift)
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            UseSkillAttack();
        }
        // �ñر� (RŰ)
        else if (Input.GetKeyDown(KeyCode.R))
        {
            UseUltimateAttack();
        }
    }

    // ��Ÿ: ������ ���� ���� �ִ� �� �� ���� ����� ������ ������ ����
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
                Debug.Log($"[��Ÿ] ������ {basicAttackDamage}�� �������� �������ϴ�. ���� ü��: {enemy.GetCurrentHealth()}");
            }
        }
        yield return new WaitForSeconds(0.3f);
        currentState = PlayerState.Idle;
    }

    // Ư�� ����: ������ ���� ���� �ִ� �� �� ���� ����� ������ ������ ����
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
                Debug.Log($"[Ư�� ����] ������ {specialAttackDamage}�� �������� �������ϴ�. ���� ü��: {enemy.GetCurrentHealth()}");
            }
        }
        yield return new WaitForSeconds(0.3f);
        currentState = PlayerState.Idle;
    }

    // ��ų ����: �÷��̾� �ֺ� skillRange ���� �� �� ���� ����� ������ ������ ����
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
                Debug.Log($"[��ų] ������ {skillDamage}�� �������� �������ϴ�. ���� ü��: {enemy.GetCurrentHealth()}");
            }
        }
        currentState = PlayerState.Idle;
    }

    // �ñر� ����: �÷��̾� �ֺ� ultimateRange ���� �� �� ���� ����� ������ ������ ����
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
                Debug.Log($"[�ñر�] ������ {ultimateDamage}�� �������� �������ϴ�. ���� ü��: {enemy.GetCurrentHealth()}");
            }
        }
        currentState = PlayerState.Idle;
    }

    // ���� (Dash) ó��
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

    // �޴� ��� (ESC Ű)
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
        Debug.Log("�κ�� �̵� (���� ����)");
        // SceneManager.LoadScene("LobbyScene");
    }
}
