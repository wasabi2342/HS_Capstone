using UnityEngine;
using System.Collections;
using Photon.Pun;


public enum WhitePlayerState { Idle, Run, BasicAttack, Hit, Dash, Skill, Ultimate, Guard, Parry, Counter, Stun, Revive, Death }

public class WhitePlayerController : ParentPlayerController
{
    //  �⺻ �̵� �� ü�� ���� 
    //[Header("�̵� �ӵ�")]
    //public float speedHorizontal = 5f;
    //public float speedVertical = 5f;

    [Header("�뽬 ����")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    //private float lastDashClickTime = -Mathf.Infinity;

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
    //private bool isGuarding = false;
    //private bool isParrying = false;

    [Header("Counter (�ߵ�) ����")]
    public int counterDamage = 20;

    // ���� ������Ʈ 
    private Animator animator;

    protected override void Awake()

    {
        AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();

        base.Awake();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�! (WhitePlayerController)");
        }
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
        //HandleDash();
        HandleMovement();
        // ����/��ų, ���� ���� ���� ��ũ��Ʈ���� ȣ��
    }

    // �Է� ó�� ����
    // WhitePlayercontroller_event.cs���� ȣ���Ͽ� �̵� �Է��� ����
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    // �̵� ó��
    private void HandleMovement()
    {
        if (currentState == WhitePlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);

        if (isMoving)
        {
            if (nextState < WhitePlayerState.Run)
            {
                nextState = WhitePlayerState.Run;
                if (!animator.GetBool("Pre-Input"))
                {
                    animator.SetBool("Pre-Input", true);
                    if (PhotonNetwork.IsConnected)
                    {
                        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    }
                }
            }
            else if (nextState > WhitePlayerState.Run)
            {
                if (animator.GetBool("run"))
                {
                    animator.SetBool("run", false);
                }
            }
        }
        else
        {
            if (nextState == WhitePlayerState.Run)
            {
                nextState = WhitePlayerState.Idle;
            }
            if (animator.GetBool("run"))
            {
                animator.SetBool("run", false);
            }
            return;

        }


        if (currentState != WhitePlayerState.Run)
            return;



        if (isMoving)
        {
            Vector3 moveDir;
            moveDir = (Mathf.Abs(v) > 0.01f) ? new Vector3(h, 0, v).normalized : new Vector3(h, 0, 0).normalized;
            transform.Translate(moveDir * runTimeData.moveSpeed * Time.deltaTime, Space.World);
        }

        if (animator != null)
        {
            animator.SetFloat("moveX", h);
            //animator.SetFloat("moveY", v);
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

    // �뽬 ó��

    public void HandleDash()
    {
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Dash)
            return;
        if (!isDashReady)
            return;
        currentState = WhitePlayerState.Dash;
        animator.ResetTrigger("run");

        animator.SetBool("dash", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "dash", true);
        }
        Vector3 dashDir = new Vector3(moveInput.x, 0, 0);

        if (dashDir == Vector3.zero)
        {
            dashDir = Vector3.right;
        }
   
        StartCoroutine(DoDash(dashDir));
    }

    private IEnumerator DoDash(Vector3 dashDir)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dashDir.normalized * dashDistance;
        yield return null;
        transform.position = targetPos;
    }


    public void HandleNormalAttack()
    {

        if (currentState != WhitePlayerState.Death)
        {
            if (currentState == WhitePlayerState.Parry)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("basicattack", true);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "basicattack", true);
                }
                //photonView.RPC("PlayAnimation", RpcTarget.All, "basicattack");
                currentState = WhitePlayerState.Counter;
                return;
            }
            else if (nextState < WhitePlayerState.BasicAttack)
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
                nextState = WhitePlayerState.BasicAttack;
            }

            if (attackStack >= 4)
            {
                animator.SetBool("Pre-Attack", false);
                animator.SetBool("Pre-Input", false);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", false);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
                }
                currentState = WhitePlayerState.Idle;
                attackStack = 0;
                AttackStackUpdate?.Invoke(attackStack);
                Debug.Log("���� ���� 4 ����: �޺� ���� �� �ʱ�ȭ");
                return;
            }

            if (currentState == WhitePlayerState.BasicAttack)
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("Pre-Input", true);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                }
            }
        }
    }

    // Ư�� ����
    public void HandleSpecialAttack()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (isShiftReady && nextState < WhitePlayerState.Skill)
            {
                nextState = WhitePlayerState.Skill;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
            }

        }
    }

    // �ñر� ���� 
    public void HandleUltimateAttack()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (isUltimateReady && nextState < WhitePlayerState.Ultimate)
            {

                nextState = WhitePlayerState.Ultimate;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
            }
        }
    }


    public WhitePlayerAttackZone AttackCollider;

    // ���� �ִϸ��̼� �̺�Ʈ�� ���� (WhitePlayerController_AttackStack���� ȣ��) 

    public IEnumerator CoStartSkillCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
    {
        isShiftReady = false;
        ShiftCoolDownUpdate?.Invoke(shiftCoolDown);
        yield return new WaitForSeconds(shiftCoolDown);
        isShiftReady = true;
    }

    public IEnumerator CoStartUltimateCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
    {
        isUltimateReady = false;
        UltimateCoolDownUpdate?.Invoke(ultimateCoolDown);
        yield return new WaitForSeconds(ultimateCoolDown);
        isUltimateReady = true;
    }

    public IEnumerator CoStartGuardCoolDown() // �̺�Ʈ Ŭ������ ��Ÿ�� üũ
    {
        isMouseRightSkillReady = false;
        MouseRightSkillCoolDownUpdate?.Invoke(mouseRightCoolDown);
        yield return new WaitForSeconds(mouseRightCoolDown);
        isMouseRightSkillReady = true;
    }

    public void OnAttackPreAttckStart()
    {
        animator.SetBool("CancleState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        }
        Debug.Log("���� ����");
    }

    public void OnAttackPreAttckEnd()
    {
        animator.SetBool("CancleState", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        }
        Debug.Log("���� ����");
    }

    public void OnMoveFront(float value)
    {
        transform.Translate((GetMouseWorldPosition() - transform.position).normalized * value);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            return new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
        }

        return Vector3.zero;
    }

    public void OnAttack1DamageStart()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower;
            }
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack1: ������ ����");
    }

    public void OnSkillCollider()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 1.7f;
            }
            AttackCollider.EnableSkillAttackCollider(true, animator.GetBool("Right"));
        }
    }

    public void OffSkillCollider()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableSkillAttackCollider(false);
        }
    }

    public void OnCounterCollider()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * runTimeData.attackSpeed;
            }
            AttackCollider.EnableCounterAttackCollider(true, animator.GetBool("Right"));
        }
    }

    public void OffCounterCollider()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableCounterAttackCollider(true, animator.GetBool("Right"));
        }
    }

    public void OnLastAttackStart()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        animator.SetBool("CancleState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        }
        Debug.Log("�ĵ� ����");
    }

    public void CreateUltimateEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                SkillEffect skillEffect = PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Right", transform.position + new Vector3(8.5f, 0, 0), Quaternion.identity).GetComponent<SkillEffect>();
                if (photonView.IsMine)
                {
                    skillEffect.Init(runTimeData.attackPower * 1.7f, AttackCollider.StartHitlag);
                }
            }
            else
            {
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>("SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Right"), transform.position + new Vector3(8.5f, 0, 0), Quaternion.identity);
                skillEffect.Init(runTimeData.attackPower * 1.7f, AttackCollider.StartHitlag);
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                SkillEffect skillEffect = PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Left", transform.position + new Vector3(-8.5f, 0, 0), Quaternion.identity).GetComponent<SkillEffect>();
                if (photonView.IsMine)
                {
                    skillEffect.Init(runTimeData.attackPower * 1.7f, AttackCollider.StartHitlag);
                }
            }
            else
            {
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>("SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Left"), transform.position + new Vector3(-8.5f, 0, 0), Quaternion.identity);
                skillEffect.Init(runTimeData.attackPower * 1.7f, AttackCollider.StartHitlag);
            }
        }
    }

    public void GetUltimateBonus()
    {
        Debug.Log("�ñر� ���� ����");
    }

    public void UltimateMove(float distance)
    {
        transform.position += new Vector3(distance, 0, 0);
    }

    public void OnAttackAllowNextInput()
    {
        animator.SetBool("FreeState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", true);
        }
        Debug.Log("��������");
    }

    public void OnAttackAnimationEnd()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
        animator.SetBool("Pre-Attack", false);
        animator.SetBool("FreeState", false);
        animator.SetBool("CancleState", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", false);
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        }
        Debug.Log(" �ִϸ��̼� ����");
    }


    public void OnAttack2DamageStart()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower;
            }
            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack2: ������ ����");
    }

    public void OnAttack3DamageStart()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 0.7f;
            }
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
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 0.7f;
            }

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack3: �ι�° �ݶ��̴� ����");
    }


    public void OnAttack4DamageStart()
    {
        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 1.5f;
            }
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
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 1.5f;
            }

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack4: �ι�° �ݶ��̴� ����");
    }


    public void InitAttackStak()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }

    // ����/�и� ó��
    public void HandleGuard()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (isMouseRightSkillReady && nextState < WhitePlayerState.Guard)
            {

                nextState = WhitePlayerState.Guard;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
            }
        }
    }

    // �ǰ� �� ��� ó��
    public override void TakeDamage(float damage)
    {
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Stun)
        {
            return;
        }
        if (isInvincible)
        {
            if (currentState == WhitePlayerState.Guard)
            {
                animator.SetBool("parry", true);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "parry", true);
                }
                //photonView.RPC("PlayAnimation", RpcTarget.All, "parry");

                currentState = WhitePlayerState.Parry;
                isMouseRightSkillReady = true;
                MouseRightSkillCoolDownUpdate?.Invoke(0);
                StopCoroutine("CoStartGuardCoolDown");
                return;
            }
            return;
        }

        base.TakeDamage(damage);

        Debug.Log("�÷��̾� ü��: " + runTimeData.currentHealth);

        if (runTimeData.currentHealth <= 0)
        {
            if (currentState != WhitePlayerState.Stun)
            {
                EnterStunState();
            }
        }
        else
        {
            if (isSuperArmor)
            {
                return;
            }
            else
            {
                //photonView.RPC("PlayAnimation", RpcTarget.All, "hit");

                animator.SetBool("hit", true);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "hit", true);
                }
                currentState = WhitePlayerState.Hit;
            }
            //StartCoroutine(CoHitReaction());
        }
    }
    [PunRPC]
    public override void DamageToMaster(float damage)
    {
        base.DamageToMaster(damage);
    }

    [PunRPC]
    public override void UpdateHP(float hp)
    {
        base.UpdateHP(hp);
        Debug.Log(photonView.ViewID + "�÷��̾� ü��: " + runTimeData.currentHealth);
    }


    // ����
    private Coroutine stunCoroutine;
    private void EnterStunState()
    {
        currentState = WhitePlayerState.Stun;
        Debug.Log("�÷��̾� ����");
        animator.SetBool("stun", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "stun", true);
        }
        //photonView.RPC("PlayAnimation", RpcTarget.All, "stun");

        // ���� ���� 30�� ������ �ڷ�ƾ 
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(CoStunDuration());
    }

    private IEnumerator CoStunDuration()
    {
        float stunDuration = 30f; // ���� �������� 5�ʷ� �ص�, ���߿� 30�ʷ� �����ϸ� �˴ϴٿ�.
        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        TransitionToDeath();
    }

    // ��Ȱ
    public void Revive()
    {
        // ���� ���� ���¶�� �ڷ�ƾ�� �����ϰ� ���¸� Idle�� ��ȯ
        if (currentState == WhitePlayerState.Stun)
        {
            if (stunCoroutine != null)
            {
                StopCoroutine(stunCoroutine);
                stunCoroutine = null;
            }
            currentState = WhitePlayerState.Idle;
            animator.SetBool("revive", true);
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("SyncBoolParameter", RpcTarget.Others, "revive", true);
            }
            //photonView.RPC("PlayAnimation", RpcTarget.All, "revive");

            // ü���� 20���� ȸ��
            photonView.RPC("UpdateHP", RpcTarget.All, 20f);
            Debug.Log("�÷��̾� ��Ȱ");
        }
    }

    // ��� ���·� ��ȯ
    private void TransitionToDeath()
    {
        currentState = WhitePlayerState.Death;
        Debug.Log("�÷��̾� ���");
        if (animator != null)
        {
            animator.SetBool("die", true);
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("SyncBoolParameter", RpcTarget.Others, "die", true);
            }
        }
    }

    public override void EnterInvincibleState()
    {
        base.EnterInvincibleState();
    }

    public override void ExitInvincibleState()
    {
        base.ExitInvincibleState();
    }

    public override void EnterSuperArmorState()
    {
        base.EnterSuperArmorState();
    }

    public override void ExitSuperArmorState()
    {
        base.ExitSuperArmorState();
    }

    [PunRPC]
    void PlayAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    [PunRPC]
    public void SyncBoolParameter(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }

    public void SetBoolParameter(string parameter, bool value)
    {
        photonView.RPC("SyncBoolParameter", RpcTarget.Others, parameter, value);
    }

    [PunRPC]
    public void SyncIntParameter(string parameter, int value)
    {
        animator.SetInteger(parameter, value);
    }

    public void SetIntParameter(string parameter, int value)
    {
        photonView.RPC("SyncIntParameter", RpcTarget.Others, parameter, value);
    }
}
