using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //  �⺻ �̵� �� ü�� ���� 
    [Header("�̵� �ӵ�")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("�뽬 ����")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    private float lastDashClickTime = -Mathf.Infinity;
    private bool isDashing = false;

    [Header("�߽��� ����")]
    [Tooltip("�⺻ CenterPoint (�ִϸ��̼� �̺�Ʈ ��� ���)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8���� CenterPoint �迭 (����: 0=��, 1=���, 2=������, 3=����, 4=�Ʒ�, 5=����, 6=����, 7=�»�)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    [Header("�÷��̾� ü��")]
    public int maxHealth = 100;
    private int currentHealth;

    // �̵� �Է� �� ����
    private Vector2 moveInput;
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Hit, Guard, Parry, Death }
    private PlayerState currentState = PlayerState.Idle;

    // ����/�޺� ���� (AttackStack �Ķ���� ���)
    [Header("����/�޺� ����")]
    [Tooltip("�޺� ���� �Է��� ��ٸ��� �ð�(��) (2�� ���� ��Ŭ���ؾ� ���� �������� �Ѿ)")]
    public float comboInputTime = 2f;
    private int attackStack = 0;         // 1~4�ܰ� �޺�
    private bool isAttacking = false;
    private bool canStartupCancel = true;  // ���� ���� ĵ�� ���� (�ʿ� �� ���)

    [Header("��Ŭ�� ����/�и� ����")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;
    private bool isGuarding = false;
    private bool isParrying = false;

    [Header("Counter (�ߵ�) ����")]
    public int counterDamage = 20;

    // ���� ������Ʈ 
    private Animator animator;

    private void Awake()
    {

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�! (WhitePlayerController)");
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
        // ����/��ų, ���� ���� ���� ��ũ��Ʈ���� ȣ��
    }

    // �Է� ó�� ����
    // WhitePlayercontroller_event.cs���� ȣ���Ͽ� �̵� �Է��� ����
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    // === �̵� ó�� ===
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

    // === �뽬 ó�� ===
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

    // ��Ÿ ����(1��2��3��4�ܰ�)
    public void HandleNormalAttack()
    {
        // �̹� ���� ���� �ƴ϶�� �޺� ����
        if (!isAttacking)
        {
            StartCoroutine(CoStackAttack());
        }
    }


    // 1~4�ܰ� �޺��� ���������� �����ϴ� �ڷ�ƾ
    // �� �ܰ谡 ���� �� 2�� ���� ���콺 ��Ŭ���� ������ Idle�� ����
    // 4�ܰ谡 ������ ��� Idle�� ����

    private IEnumerator CoStackAttack()
    {
        isAttacking = true;

        // 1~4�ܰ踦 ���������� ����
        for (int stage = 1; stage <= 4; stage++)
        {
            attackStack = stage;
            switch (stage)
            {
                case 1:
                    currentState = PlayerState.Attack_L;
                    Debug.Log("��Ÿ ���� ���� 1�ܰ� ����");
                    break;
                case 2:
                    currentState = PlayerState.Attack_R;
                    Debug.Log("��Ÿ ���� ���� 2�ܰ� ����");
                    break;
                case 3:
                    currentState = PlayerState.Skill;
                    Debug.Log("��Ÿ ���� ���� 3�ܰ� ����");
                    break;
                case 4:
                    currentState = PlayerState.Ultimate;
                    Debug.Log("��Ÿ ���� ���� 4�ܰ� ����");
                    break;
            }

            // ���콺 ���� ���� (�ʿ��ϴٸ�)
            FaceMouseDirection();

            // Animator �Ķ���� ����
            if (animator != null)
            {
                animator.SetInteger("AttackStack", stage);
                animator.SetBool("isAttacking", true);
            }

            // �ణ�� ����(0.2��) ��� 
            yield return new WaitForSeconds(0.2f);

            // ���� ���簡 4�ܰ��� �ٷ� ����
            if (stage == 4)
            {
                // 4�ܰ� �ִϸ��̼��� ������ ��� Idle�� ���ư����� ó��
                break;
            }

            // ���� �ܰ�� �Ѿ�� ���� 2�� ���� ��Ŭ���� ��ٸ�
            float timer = 0f;
            bool nextClick = false;
            while (timer < comboInputTime)
            {
                timer += Time.deltaTime;

                // �̵�/�뽬�� �߻��ϸ� �޺� ����
                if (moveInput.magnitude > 0.01f || isDashing)
                {
                    ResetAttackStack();
                    yield break;
                }

                // ���콺 ��Ŭ�� ����
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    nextClick = true;
                    break;
                }
                yield return null;
            }

            // ���� 2�� �ȿ� ��Ŭ���� �����ٸ� �޺� ����
            if (!nextClick)
            {
                ResetAttackStack();
                yield break;
            }

            // ��Ŭ���� �־��ٸ� for ���� ���� �ܰ�� �Ѿ
        }

        // 4�ܰ���� �Ϸ��߰ų� ������ �������Ƿ� ����
        ResetAttackStack();
    }

    private void ResetAttackStack()
    {
        attackStack = 0;
        isAttacking = false;
        currentState = PlayerState.Idle;
        canStartupCancel = true;
        Debug.Log("��Ÿ ���� ���� �ʱ�ȭ �� Idle ���·� ����");

        if (animator != null)
        {
            animator.SetInteger("AttackStack", 0);
            animator.SetBool("isAttacking", false);
        }
    }

    // Ư�� ����
    public void HandleSpecialAttack()
    {
        FaceMouseDirection();
        Debug.Log("Ư�� ���� ȣ���");
        if (animator != null)
        {
            animator.SetTrigger("SpecialAttack");
        }
    }

    // �ñر� ���� 
    public void HandleUltimateAttack()
    {
        FaceMouseDirection();
        Debug.Log("�ñر� ���� ȣ���");
        if (animator != null)
        {
            animator.SetTrigger("UltimateAttack");
        }
    }

    // ���� �ִϸ��̼� �̺�Ʈ�� ���� (WhitePlayerController_AttackStack���� ȣ��) 
    public void OnAttack1StartupEnd() { Debug.Log("���� 1: ���� ����"); }
    public void OnAttack1DamageStart() { Debug.Log("���� 1: ������ ����"); }
    public void OnAttack1DamageEnd() { Debug.Log("���� 1: ������ ����"); }
    public void OnAttack1AllowNextInput() { Debug.Log("���� 1: �߰� �Է� ���"); }
    public void OnAttack1RecoveryEnd() { Debug.Log("���� 1: �ĵ� ����"); }
    public void OnAttack1AnimationEnd() { Debug.Log("���� 1: �ִϸ��̼� ����"); }

    public void OnAttack2StartupFrame1End() { Debug.Log("���� 2: ��ŸƮ�� ������ 1 ����"); }
    public void OnAttack2StartupFrame2End() { Debug.Log("���� 2: ��ŸƮ�� ������ 2 ����"); }
    public void OnAttack2DamageStart() { Debug.Log("���� 2: ������ ����"); }
    public void OnAttack2DamageEnd() { Debug.Log("���� 2: ������ ����"); }
    public void OnAttack2AllowNextInput() { Debug.Log("���� 2: �߰� �Է� ���"); }
    public void OnAttack2RecoveryEnd() { Debug.Log("���� 2: �ĵ� ����"); }
    public void OnAttack2AnimationEnd() { Debug.Log("���� 2: �ִϸ��̼� ����"); }

    // ����/�и� ó��
    public void HandleGuard()
    {
        Debug.Log("���� ȣ���");

    }

    public void HandleParry()
    {
        Debug.Log("�и� ȣ���");
    }

    public void OnCounterAttackEvent()
    {
        Debug.Log("�ߵ�(�ݰ�) ȣ���, ������: " + counterDamage);
    }

    // �ǰ� �� ��� ó��
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("�÷��̾� ü��: " + currentHealth);

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
        Debug.Log("�÷��̾� ���");
        if (animator != null)
            animator.SetBool("isDead", true);
    }

    // ��Ÿ ��ƿ��Ƽ
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

    // �� ��ȯ ����
    public void UsePortal(Vector3 exitPosition)
    {
        transform.position = exitPosition;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �� ��ȯ �� �ʱ�ȭ ó��
    }
}
