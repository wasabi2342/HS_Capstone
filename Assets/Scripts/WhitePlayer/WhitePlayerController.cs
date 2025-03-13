using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;


public enum WhitePlayerState { Idle, Run, BasicAttack, Hit, Dash, Skill, Ultimate, Guard, Parry, Counter, Death }

public class WhitePlayerController : ParentPlayerController
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




    // �̵� �Է� �� ����
    private Vector2 moveInput;
    public WhitePlayerState currentState = WhitePlayerState.Idle;
    public WhitePlayerState nextState = WhitePlayerState.Idle;

    [Header("��Ŭ�� ����/�и� ����")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;
    private bool isGuarding = false;
    private bool isParrying = false;

    [Header("Counter (�ߵ�) ����")]
    public int counterDamage = 20;

    // ���� ������Ʈ 
    private Animator animator;

    public int attackStack = 0;

    protected override void Awake()

    {
        AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();
        
        base.Awake();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�! (WhitePlayerController)");
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        currentState = WhitePlayerState.Idle;
    }

    private void Update()
    {
        if (currentState == WhitePlayerState.Death)
            return;

        UpdateCenterPoint();
        CheckDashInput();
        HandleMovement();
        // ����/��ų, ���� ���� ���� ��ũ��Ʈ���� ȣ��
        if (isMoveInput)
        {
            if (nextState < WhitePlayerState.Run)
            {
                nextState = WhitePlayerState.Run;
                animator.SetBool("Pre-Input", true);
            }
        }
    }

    public bool isMoveInput;

    // �Է� ó�� ����
    // WhitePlayercontroller_event.cs���� ȣ���Ͽ� �̵� �Է��� ����
    public void SetMoveInput(Vector2 input)
    {
        if (nextState < WhitePlayerState.Run)
        {
            nextState = WhitePlayerState.Run;
            animator.SetBool("Pre-Input", true);
        }
        moveInput = input;
        isMoveInput = (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f);
    }

    // === �̵� ó�� ===
    private void HandleMovement()
    {
        if (currentState == WhitePlayerState.Death) return;
        if (nextState != WhitePlayerState.Idle && nextState != WhitePlayerState.Run)
        {
            animator.SetBool("run", false);
            return;
        }
        if (currentState != WhitePlayerState.Run)
            return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? WhitePlayerState.Run : WhitePlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir;
            moveDir = (Mathf.Abs(v) > 0.01f) ? new Vector3(h, 0, v).normalized : new Vector3(h, 0, 0).normalized;
            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);
        }

        if (animator != null)
        {
            animator.SetBool("run", isMoving);
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


    public void HandleNormalAttack()
    {

        if (currentState != WhitePlayerState.Death)
        {
            if (currentState == WhitePlayerState.Parry)
            {
                animator.SetTrigger("basicattack");
                currentState = WhitePlayerState.Counter;
            }
            else if (nextState < WhitePlayerState.BasicAttack)
            {
                nextState = WhitePlayerState.BasicAttack;
            }
            if(currentState == WhitePlayerState.BasicAttack)
            {
                animator.SetBool("Pre-Input", true);
            }
        }
    }

    // Ư�� ����
    public void HandleSpecialAttack()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (nextState < WhitePlayerState.Skill)
            {

                nextState = WhitePlayerState.Skill;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
            }

        }
    }

    // �ñر� ���� 
    public void HandleUltimateAttack()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (nextState < WhitePlayerState.Ultimate)
            {

                nextState = WhitePlayerState.Ultimate;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
            }
        }
    }

   
    public WhitePlayerAttackZone AttackCollider;
    
    // ���� �ִϸ��̼� �̺�Ʈ�� ���� (WhitePlayerController_AttackStack���� ȣ��) 

    public void OnAttackPreAttckStart()
    {

        
        animator.SetBool("CancleState", true);
        Debug.Log("���� ����");
    }

    public void OnAttackPreAttckEnd()
    {
        animator.SetBool("CancleState", false);
    
        Debug.Log("���� ����");
    }

    public void OnMoveFront(float value)
    {
        transform.Translate(new Vector3(value, 0, 0));
    }
    public void OnAttack1DamageStart()
    {

        if (AttackCollider != null)
        {
            AttackCollider.Damage = 10f;
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack1: ������ ����");
    }


   

    public void OnLastAttackStart()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        animator.SetBool("CancleState", true);
        Debug.Log("�ĵ� ����");
    }



    public void OnAttackAllowNextInput()
    {
        animator.SetBool("FreeState", true);
        Debug.Log("��������");
    }

    public void OnAttackAnimationEnd()
    {
        attackStack = 0;
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("CancleState", false);
        Debug.Log(" �ִϸ��̼� ����");
    }

    
    public void OnAttack2DamageStart()
    {
        if (AttackCollider != null)
        {
            AttackCollider.Damage = 15f;
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack2: ������ ����");
    }

    public void OnAttack3DamageStart()
    {
        if (AttackCollider != null)
        {
            AttackCollider.Damage = 20f;
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack3: ������ ����");
    }

    public void OnCollider3Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);

            Debug.Log("Attack3: ù��° �ݶ��̴� ����");
        }

        if (AttackCollider != null)
        {
            AttackCollider.Damage = 20f;

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack3: �ι�° �ݶ��̴� ����");
    }


    public void OnAttack4DamageStart()
    {
        if (AttackCollider != null)
        {
            AttackCollider.Damage = 25f;
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack4: ������ ����");
    }

    public void OnCollider4Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);

            Debug.Log("Attack4: ù��° �ݶ��̴� ����");
        }

        if (AttackCollider != null)
        {
            AttackCollider.Damage = 25f;

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack4: �ι�° �ݶ��̴� ����");
    }


    public void InitAttackStak()
    {
        attackStack = 0;
    }

    // ����/�и� ó��
    public void HandleGuard()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (nextState < WhitePlayerState.Guard)
            {

                nextState = WhitePlayerState.Guard;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);

            }
        }

    }

    // �ǰ� �� ��� ó��
    public override void TakeDamage(float damage)
    {
        if (currentState == WhitePlayerState.Guard)
        {
            animator.SetTrigger("parry");
            currentState = WhitePlayerState.Parry;
            return;
        }

        base.TakeDamage(damage);

        Debug.Log("�÷��̾� ü��: " + currentHealth);

        if (currentHealth <= 0 && currentState != WhitePlayerState.Death)
        {
            Die();
        }
        else
        {
            //StartCoroutine(CoHitReaction());
        }
    }

    public override void DamageToMaster(float damage)
    {
        base.DamageToMaster(damage);
    }

    public override void UpdateHP(float hp)
    {
        base.UpdateHP(hp);
    }

    //private IEnumerator CoHitReaction()
    //{
    //    currentState = WhitePlayerState.Hit;
    //    if (animator != null)
    //        animator.SetBool("hit", true);
    //    yield return new WaitForSeconds(0.5f);
    //    if (animator != null)
    //        animator.SetBool("hit", false);
    //    if (currentState != WhitePlayerState.Death)
    //        currentState = WhitePlayerState.Idle;
    //}

    private void Die()
    {
        currentState = WhitePlayerState.Death;
        Debug.Log("�÷��̾� ���");
        if (animator != null)
            animator.SetBool("die", true);
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
