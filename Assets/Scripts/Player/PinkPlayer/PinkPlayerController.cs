using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

public enum PinkPlayerState { Idle, Run, tackle, BasicAttack, Hit, Dash, Skill, Ultimate, R_Idle, R_hit1, R_hit2, R_hit3, R_finish, Charge1, Charge2, Charge3, Stun, Revive, Death }

public class PinkPlayerController : ParentPlayerController
{
    [Header("�뽬 ����")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    public float dashDuration = 0.2f;
    private Vector3 dashDirection;
    private Vector3 facingDirection = Vector3.right;
    //private float lastDashClickTime = -Mathf.Infinity;

    [Header("�߽��� ����")]
    [Tooltip("�⺻ CenterPoint (�ִϸ��̼� �̺�Ʈ ��� ���)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8���� CenterPoint �迭 (����: 0=��, 1=���, 2=������, 3=����, 4=�Ʒ�, 5=����, 6=����, 7=�»�)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    [Header("����Ʈ ��ȯ ����")]
    [SerializeField] private GameObject servantPrefab;
    [SerializeField] private Vector3 servantSpawnOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private int maxServants = 8;

    private List<ServantFSM> summonedServants = new List<ServantFSM>();
    [Header("�ñر� ����")]
    [SerializeField] private float ultimateDuration = 5f;
    private bool isUltimateActive = false;

    // �̵� �Է� �� ����
    private Vector2 moveInput;
    public PinkPlayerState currentState = PinkPlayerState.Idle;
    public PinkPlayerState nextState = PinkPlayerState.Idle;

    private List<ServantFSM> myServants = new List<ServantFSM>();
    private const int MAX_SERVANTS = 8;

    protected override void Awake()

    {

        base.Awake();
        facingDirection = Vector3.right;
    }

    protected override void Start()
    {
        base.Start();

        currentState = PinkPlayerState.Idle;

        if (photonView.IsMine)
        {
            if (photonView.IsMine)
            {

                if (stunOverlay != null) stunOverlay.enabled = false;
                if (stunSlider != null) stunSlider.gameObject.SetActive(false);
                if (hpBar != null) hpBar.enabled = true;

                gaugeInteraction = GetComponentInChildren<GaugeInteraction>();

                var eventController = GetComponent<PinkPlayercontroller_event>();
                if (eventController != null)
                {
                    //eventController.OnInteractionEvent += HandleReviveInteraction;
                }
            }
        }
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

        if (currentState == PinkPlayerState.Death)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                RoomManager.Instance.SwitchCameraToNextPlayer();
            }
            return;
        }


        //UpdateCenterPoint();
        HandleMovement();

    }

    // �Է� ó�� ����

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
     
    }

    // �̵� ó��

    private void HandleMovement()
    {
        if (currentState == PinkPlayerState.Death || currentState == PinkPlayerState.Stun)
        {
            return;
        }

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);

        if (isMoving)
        {
            if (nextState < PinkPlayerState.Run)
            {
                nextState = PinkPlayerState.Run;
                if (!animator.GetBool("Pre-Input"))
                {
                    animator.SetBool("Pre-Input", true);
                    if (PhotonNetwork.IsConnected)
                    {
                        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    }
                }
            }
            else if (nextState > PinkPlayerState.Run)
            {
                if (animator.GetBool("run"))
                {
                    animator.SetBool("run", false);
                }
            }
        }
        else
        {
            if (nextState == PinkPlayerState.Run)
            {
                nextState = PinkPlayerState.Idle;
            }
            if (animator.GetBool("run"))
            {
                animator.SetBool("run", false);
            }
            return;

        }


        if (currentState != PinkPlayerState.Run
     && !(currentState == PinkPlayerState.R_Idle && animator.GetBool("run")))
        {
            return;
        }



        if (currentState == PinkPlayerState.Run
     || (currentState == PinkPlayerState.R_Idle && animator.GetBool("run")))
        {
            // ���⼭�� ���� ���� ����
            if (h > 0.01f) facingDirection = Vector3.right;
            else if (h < -0.01f) facingDirection = Vector3.left;

            // �̵�
            Vector3 moveDir = (Mathf.Abs(v) > 0.01f)
                ? new Vector3(h, 0, v).normalized
                : new Vector3(h, 0, 0).normalized;
            rb.MovePosition(rb.position + moveDir * runTimeData.moveSpeed * Time.fixedDeltaTime);

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
        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    private Vector3 lastFacingDirection = Vector3.right;



    // �뽬 ó��
 
    public void HandleDash()
    {
        if (currentState == PinkPlayerState.Death
         || currentState == PinkPlayerState.Dash
         || currentState == PinkPlayerState.Stun)
            return;

        if (!cooldownCheckers[(int)Skills.Space].CanUse())
            return;

        nextState = PinkPlayerState.Dash;
        //animator.ResetTrigger("run");
        //animator.SetBool("dash", true);
        //if (PhotonNetwork.IsConnected)
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "dash", true);

        // ���⼭�� ������ facingDirection ���
        dashDirection = facingDirection;

        //StartCoroutine(DoDash());
    }


    // ��Ÿ
    public void HandleNormalAttack()
    {

        if (currentState != PinkPlayerState.Death || currentState == PinkPlayerState.Stun)
        {

            if (currentState == PinkPlayerState.R_Idle)
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
                currentState = PinkPlayerState.R_hit1;
                return;
            }

            if (currentState == PinkPlayerState.R_hit1 || currentState == PinkPlayerState.R_hit2 || currentState == PinkPlayerState.R_hit3)
            {
                //animator.SetBool("basicattack", true);
                animator.SetBool("Pre-Input", true);
                //Vector3 mousePos = GetMouseWorldPosition();
                //animator.SetBool("Right", mousePos.x > transform.position.x);
                //if (PhotonNetwork.IsConnected)
                //{
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "basicattack", true);
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                //}
                //currentState = PinkPlayerState.R_hit2;
                //return;
            }

            //else if (currentState == PinkPlayerState.R_hit1 && animator.GetInteger("attackStack") > 2)
            //{
            //    animator.SetBool("basicattack", true);
            //    Vector3 mousePos = GetMouseWorldPosition();
            //    animator.SetBool("Right", mousePos.x > transform.position.x);
            //    if (PhotonNetwork.IsConnected)
            //    {
            //        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "basicattack", true);
            //        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
            //    }
            //    currentState = PinkPlayerState.R_hit3;
            //    return;
            //}

            if (nextState <= PinkPlayerState.BasicAttack)
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("Pre-Input", true);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
                currentState = PinkPlayerState.BasicAttack;
                nextState = PinkPlayerState.BasicAttack;
            }

            if (attackStack == 2)
            {

            }

            if (attackStack > 2)
            {
                animator.SetBool("Pre-Attack", false);
                animator.SetBool("Pre-Input", false);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", false);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
                }
                //currentState = PinkPlayerState.Idle;
                //attackStack = 0;
                AttackStackUpdate?.Invoke(attackStack);
                Debug.Log("���� ���� 2 ����: �޺� ���� �� �ʱ�ȭ");
                return;
            }

            if (currentState == PinkPlayerState.BasicAttack)
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

    // ��Ŭ��

    public void HandleCharge()
    {
        if (currentState != PinkPlayerState.Death)
        {
            if (currentState == PinkPlayerState.BasicAttack && animator.GetBool("CancleState2"))
            {


                currentState = PinkPlayerState.tackle;

                animator.SetBool("tackle", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "tackle", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }

                animator.SetBool("basicattack", false);
                return;

            }
        }
    }

    // ����

    private bool isCharging = false;
    private int chargeLevel = 0;

    public float physicalAtk;
    public float magicAtk;

    // ��Ŭ�� ������ �� (���� ����)
    public void StartCharge()
    {
        if (isCharging || currentState == PinkPlayerState.Death)
            return;
        if (currentState == PinkPlayerState.tackle)
            return;
        if (currentState == PinkPlayerState.Charge1 || currentState == PinkPlayerState.Charge2 || currentState == PinkPlayerState.Charge3)
            return;
        if (nextState > PinkPlayerState.Charge1)
            return;

        isCharging = true;
        chargeLevel = 1;
        nextState = PinkPlayerState.Charge1;

        Vector3 mousePos = GetMouseWorldPosition();
        bool isRight = mousePos.x > transform.position.x;

        animator.SetBool("Right", isRight);
        //animator.SetBool("isCharging", true); 
        //animator.SetInteger("chargeLevel", chargeLevel);

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", isRight);
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "isCharging", true);
            photonView.RPC("SyncIntParameter", RpcTarget.Others, "chargeLevel", chargeLevel);
        }


    }

    // ��Ŭ���� ������ �� (���� ����)
    public void ReleaseCharge()
    {
        if (!isCharging || currentState == PinkPlayerState.Death)
            return;

        if (!(currentState == PinkPlayerState.Charge1 || currentState == PinkPlayerState.Charge2 || currentState == PinkPlayerState.Charge3))
            return;

        isCharging = false;

        animator.SetBool("isCharging", false);
        animator.SetTrigger("ReleaseCharge");

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "isCharging", false);
            photonView.RPC("PlayAnimation", RpcTarget.Others, "ReleaseCharge");
        }
    }


    // �����Ӻ� ���� ���� ���� (�ִϸ��̼� �̺�Ʈ�� ȣ��)
    public void SetChargeLevel(int level)
    {
        chargeLevel = level;
        animator.SetInteger("chargeLevel", level); // �ִϸ����� �Ķ���� ������Ʈ

        switch (level)
        {
            case 1:
                currentState = PinkPlayerState.Charge1;
                break;
            case 2:
                currentState = PinkPlayerState.Charge2;
                break;
            case 3:
                currentState = PinkPlayerState.Charge3;
                break;
        }

        Debug.Log($"���� ����: {chargeLevel}, nextState: {nextState}");
    }





    // Ư�� ����
    public void HandleSpecialAttack()
    {
        if (currentState != PinkPlayerState.Death)
        {
            if (cooldownCheckers[(int)Skills.Shift_L].CanUse() && nextState < PinkPlayerState.Skill)
            {
                if (myServants.Count >= MAX_SERVANTS)
                {
                    Debug.Log("�ִ� ��ȯ�� ������ �����߽��ϴ�.");
                    return;
                }

                nextState = PinkPlayerState.Skill;
                animator.SetBool("Pre-Attack", true);
                animator.SetBool("Pre-Input", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);

                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                    photonView.RPC("RPC_SpawnServant", RpcTarget.MasterClient, photonView.ViewID);
                }
            }

        }
    }

    // ���� ��ȯ�� ��ȯ ���� �и�
    [PunRPC]
    private void RPC_SpawnServant(int ownerViewID)
    {
        Vector3 spawnPos = transform.position + servantSpawnOffset;
        GameObject servant = PhotonNetwork.Instantiate(servantPrefab.name, spawnPos, Quaternion.identity);
        ServantFSM servantFSM = servant.GetComponent<ServantFSM>();

        servantFSM.photonView.RPC("RPC_SetOwner", RpcTarget.AllBuffered, ownerViewID);
        photonView.RPC("RPC_RegisterServant", RpcTarget.AllBuffered, servantFSM.photonView.ViewID, ownerViewID);
    }
    [PunRPC]
    private void RPC_RegisterServant(int servantViewID, int ownerViewID)
    {
        if (photonView.ViewID == ownerViewID)
        {
            PhotonView servantPV = PhotonView.Find(servantViewID);
            if (servantPV != null)
            {
                ServantFSM servantFSM = servantPV.GetComponent<ServantFSM>();
                if (servantFSM != null)
                    myServants.Add(servantFSM);
            }
        }
    }
    /// <summary>ServantFSM �ν��Ͻ��� ����Ʈ�� ����մϴ�.</summary>
    public void AddServantToList(ServantFSM servant)
    {
        if (servant != null && !summonedServants.Contains(servant))
            summonedServants.Add(servant);
    }

    /// <summary>ServantFSM �ν��Ͻ��� ����Ʈ���� �����մϴ�.</summary>
    public void RemoveServantFromList(ServantFSM servant)
    {
        if (servant != null)
            summonedServants.Remove(servant);
    }
    private int servantCount = 0;
    private const int maxUltimateStacks = 8;

    // �ñر�
    /// <summary>
    /// R Ű �Է��� ������ ��,
    /// ���� ���¿� ���� �ñر� ���� or ������ �ϰ� ����
    /// </summary>
    public void OnUltimateInput(InputAction.CallbackContext context)
    {
        if (animator == null || currentState == PinkPlayerState.Death)
            return;
        if (!photonView.IsMine) return;
        // ��ư ���� ��������
        if (context.started && !isUltimateActive && cooldownCheckers[(int)Skills.R].CanUse())
        {
            // MasterClient �������� ���� �ñر� ����
            photonView.RPC(nameof(RPC_UseUltimate), RpcTarget.MasterClient);
        }
        if (context.started)
        {
            // ���� ���� ����
            R_attackStack = 0;
            currentState = PinkPlayerState.R_Idle;
            animator.SetBool("ultimate", true);


            Vector3 mousePos = GetMouseWorldPosition();
            bool isRight = mousePos.x > transform.position.x;
            animator.SetBool("Right", isRight);


            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC(
                    "SyncBoolParameter",
                    RpcTarget.Others,
                    "ultimate",
                    true
                );
                photonView.RPC(
                    "SyncIntParameter",
                    RpcTarget.Others,
                    "R_attackStack",
                    R_attackStack
                );
                photonView.RPC(
                    "SyncBoolParameter",
                    RpcTarget.Others,
                    "Right",
                    isRight
                );
            }
        }
    }

    [PunRPC]
    private void RPC_UseUltimate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 1) myServants �� ���� ForceKill RPC ȣ��
        foreach (var s in myServants)
        {
            if (s != null && s.photonView != null)
                s.photonView.RPC("ForceKill", RpcTarget.AllBuffered);
        }
        myServants.Clear();

        // 2) (���� ������ �� �Լ��� �ξ����ϴ�)
        ApplyUltimateBuff();

        // 3) �ñر� �ִ�/���� ����
        if (photonView.IsMine)
        {
            currentState = PinkPlayerState.Ultimate;
            animator.SetTrigger("ultimate");
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "ultimate", true);
        }
    }
    /// <summary>
    /// ����� ��ȯ�� ������ ���� ��ȭ ȿ�� ����
    /// (���� �� �Լ� - �ʿ�� ���⿡ ���� �ڵ带 �߰��ϼ���)
    /// </summary>
    private void ApplyUltimateBuff()
    {
        // TODO: �ñر� ���� ����
    }
    // �ñر� ���� ����
    public void HandleUltimateStart()
    {
        if (currentState == PinkPlayerState.Death || !cooldownCheckers[(int)Skills.R].CanUse())
            return;

        nextState = PinkPlayerState.Ultimate;
        animator.SetBool("Pre-Attack", true);
        animator.SetBool("Pre-Input", true);
        bool isRight = GetMouseWorldPosition().x > transform.position.x;
        animator.SetBool("Right", isRight);

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", true);
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", isRight);
        }
    }

    // �ñر� ���ӽð��� �� �ǰų� R�� �ٽ� ���� �� ȣ��
    public void HandleUltimateEndOrFinal()
    {
        int servantCount = myServants.Count;
        foreach(var servant in myServants)
        {
            if (servant != null)
            {
                servant.photonView.RPC("ForceKill", RpcTarget.MasterClient);
            }
        }
        myServants.Clear();
        ApplyUltimateBuff(servantCount);
        // �ִϸ����Ϳ��� ������ �ϰ�(FinalStrike) ���·� ����
        currentState = PinkPlayerState.Ultimate;
        animator.SetTrigger("FinalStrike");
        animator.SetTrigger("FinalStrike");
        if (PhotonNetwork.IsConnected)
            photonView.RPC("PlayAnimation", RpcTarget.Others, "FinalStrike");
    }
    void ApplyUltimateBuff(int servantCount)
    {
        //����� ��ȯ�� ������ ���� ��ȭ ȿ��
    }
    public int R_attackStack;

    // �ִϸ��̼� �̺�Ʈ�� ��Ÿ ���� �������ֱ�
    public void OnAttackStack()
    {
        //if (currentState != PinkPlayerState.BasicAttack) return;

        R_attackStack++;
        Debug.Log(R_attackStack);
        animator.SetInteger("R_attackStack", R_attackStack);
        AttackStackUpdate?.Invoke(R_attackStack);
        Debug.Log($"��Ÿ ���� ����: {R_attackStack}");
    }

    // ���� ���� �̺�Ʈ�� ȣ��
    public void OnUltimateCastComplete()
    {
        int stacks = servantCount;
        float totalShield = 30f * stacks;
        float totalDuration = 2f * stacks;

        if (stacks > 0)
        {
            AddShield(totalShield, totalDuration);
            Debug.Log($"�ñر� ����: +{totalShield}HP, ���� {totalDuration}s (���� {stacks})");
            servantCount = 0;   // ���� �ʱ�ȭ
        }
    }


    //public WhitePlayerAttackZone AttackCollider;

    // ���� �ִϸ��̼� �̺�Ʈ�� ���� (WhitePlayerController_AttackStack���� ȣ��) 

    public override void StartMouseRCoolDown()
    {
        base.StartMouseRCoolDown();
    }

    public override void StartShiftCoolDown()
    {
        base.StartShiftCoolDown();
    }

    public override void StartUltimateCoolDown()
    {
        base.StartUltimateCoolDown();
    }

    public override void StartAttackCooldown()
    {
        base.StartAttackCooldown();
    }

    public override void StartSpaceCooldown()
    {
        base.StartSpaceCooldown();
    }

    // �ִϸ��̼� �̺�Ʈ �Լ�

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

    public void OnAttackLastAttckStart()
    {
        animator.SetBool("CancleState2", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", true);
        }
        Debug.Log("�ĵ� ����");
    }

    public void OnAttackLastAttckEnd()
    {
        animator.SetBool("CancleState2", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", false);
        }
        Debug.Log("�ĵ� ����");
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

    public void OnMoveFront2(float value)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        Vector3 movement = dashDirection * value;
        rb.MovePosition(rb.position + movement);
    }


    #region ��ų ����Ʈ ����

    // �ñر� ����Ʈ ����
    public void CreateUltimateEffectStart()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // �ñر� ������ ���
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;
        string effectPath;
        // ����Ʈ ��� �� ��ġ ����

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_idle_right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_idle_left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, true);

        // Photon�� ���� ���̵� �ƴϵ�, ���ÿ��� ����Ʈ�� �����ϴ� �ڵ�
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // init �޼��� ȣ��
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.R].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // �ñر� ��Ʈ ����Ʈ ����
    public void CreateUltimateEffectHit()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // �ñر� ������ ���
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;

        // R_attackStack�� 3���� Ŭ����
        int maxR_attackStackEffect = Mathf.Min(R_attackStack, 2);

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_R_hit{maxR_attackStackEffect}_right_front_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_R_hit{maxR_attackStackEffect}_left_front_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, true);

        var prefab = Resources.Load<SkillEffect>(effectPath);
        if (prefab == null)
        {
            Debug.LogError($"[R-Effect] Prefab �ε� ����! ��� Ȯ���ϼ���: {effectPath}");
            return;
        }
        Debug.Log($"[R-Effect] Prefab �ε� ����: {prefab.name}");

        // Photon�� ���� ���̵� �ƴϵ�, ���ÿ��� ����Ʈ�� �����ϴ� �ڵ�
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // Init �޼��� ȣ��, ���⼭ �ٽ� ȣ������ ���� ����� --> �Ƹ� ��ȹ ������ �� ��ĥ ����
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // �ñر� ����Ʈ ���� (Finish)
    public void CreateUltimateEffectFinish()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // �ñر� ������ ���
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower);

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_finish_right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_finish_left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, true);

        // Photon�� ���� ���̵� �ƴϵ�, ���ÿ��� ����Ʈ�� �����ϴ� �ڵ�
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // Init �޼��� ȣ��, ���⼭ �ٽ� ȣ������ ���� ����� --> �Ƹ� ��ȹ ������ �� ��ĥ ����
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // ��Ÿ ����Ʈ ����
    public void CreateBasicAttackEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        // ��Ÿ ������ ���
        float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
        float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_attack{attackStack}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_attack{attackStack}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
            effectPosition = transform.position; // ������ �� ��ġ�� �⺻ ��ġ �״�� ����
        }

        // �ٸ� Ŭ���̾�Ʈ���Ե� ����Ʈ�� �����ϵ��� RPC ȣ��
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true);
        }

        // Photon ���� ���ο� ���� ����Ʈ ����
        Debug.Log(effectPath);
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init �޼��� ȣ��
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // ��¡ ����Ʈ ����
    public void CreateChargeSkillEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        // ��¡ ��ų ������ ���
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;
        Vector3 effectPosition = transform.position + new Vector3(0f, -0.5f, 0f);

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }

        // �ٸ� Ŭ���̾�Ʈ���Ե� ����Ʈ�� �����ϵ��� RPC ȣ��
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true);
        }

        // Photon ���� ���ο� ���� ����Ʈ ����
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init �޼��� ȣ��
        //skillEffect.Init(damage, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    public void CreateChargeHitSkillEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        // ��¡ ��ų ������ ���
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }

        // �ٸ� Ŭ���̾�Ʈ���Ե� ����Ʈ�� �����ϵ��� RPC ȣ��
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true);
        }

        // Photon ���� ���ο� ������� ���ÿ��� ����Ʈ ����
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init �޼��� ȣ��
        //skillEffect.Init(damage, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));


        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // ����Ʈ ��ų ����Ʈ ����
    public void CreateShiftSkillEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        // ����Ʈ ��ų ������ ���
        float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/WhitePlayer/ShiftSkill_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/WhitePlayer/ShiftSkill_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}";
        }

        // �ٸ� Ŭ���̾�Ʈ���Ե� ����Ʈ�� �����ϵ��� RPC ȣ��
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true);
        }

        // Photon ���� ���ο� ������� ���ÿ��� ����Ʈ ����
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init �޼��� ȣ��
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }

    // ��Ŭ ����Ʈ ����
    public void CreateTackleSkillEffect()
    {

        // ��Ŭ ��ų ������ ���
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon�� ���� ������ Ȯ���Ͽ� isMine ����
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // ����Ʈ ��� �� ��ġ ����
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_tackle_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_tackle_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
        }

        // �ٸ� Ŭ���̾�Ʈ���Ե� ����Ʈ�� �����ϵ��� RPC ȣ��
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true);
        }

        // Photon ���� ���ο� ������� ���ÿ��� ����Ʈ ����
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init �޼��� ȣ��
        //skillEffect.Init(damage, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // ������ ����Ʈ�� �θ� ����
        skillEffect.transform.parent = transform;
    }


    // �����̽� ����Ʈ �����̳ʿ� ȿ���� ��Ÿ������
    public void CreateSpaceSkillEffect()
    {
        // Photon�� ���� ���� �ƴ� �� photonView.IsMine ���� false�� ó��
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/EffectContainer"), transform.position, Quaternion.identity);
        skillEffect.transform.parent = transform;
        skillEffect.Init(0, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Space].skillData.ID, this));
    }

    #endregion


    [PunRPC]
    public override void CreateAnimation(string name, Vector3 pos, bool isChild)
    {
        base.CreateAnimation(name, pos, isChild);
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

    public void InitAttackStak()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }


    // �ǰ� �� ��� ó��
    public override void TakeDamage(float damage, Vector3 attackerPos, AttackerType attackerType = AttackerType.Default)
    {
        if (currentState == PinkPlayerState.Death || currentState == PinkPlayerState.Stun)
        {
            return;
        }
        AudioManager.Instance.PlayOneShot("event:/Character/Common/Character Hit", transform.position);
        //if (isInvincible)
        //{
        //    if (currentState == PinkPlayerState.Guard)
        //    {
        //        animator.SetBool("parry", true);
        //        if (PhotonNetwork.IsConnected)
        //        {
        //            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "parry", true);
        //        }
        //        //photonView.RPC("PlayAnimation", RpcTarget.All, "parry");

        //        currentState = WhitePlayerState.Parry;
        //        cooldownCheckers[(int)Skills.Mouse_R].ResetCooldown(this);
        //        return;
        //    }
        //    return;
        //}

        base.TakeDamage(damage, attackerPos);

        Debug.Log("�÷��̾� ü��: " + runTimeData.currentHealth);

        if (runTimeData.currentHealth <= 0)
        {
            if (currentState != PinkPlayerState.Stun)
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
                currentState = PinkPlayerState.Hit;
            }
            //StartCoroutine(CoHitReaction());
        }


    }
    [PunRPC]
    public override void DamageToMaster(float damage, Vector3 attackerPos)
    {
        base.DamageToMaster(damage, attackerPos);
    }

    [PunRPC]
    public override void UpdateHP(float hp)
    {
        base.UpdateHP(hp);
        Debug.Log(photonView.ViewID + "�÷��̾� ü��: " + runTimeData.currentHealth);

        if (runTimeData.currentHealth <= 0)
        {
            if (currentState != PinkPlayerState.Stun)
            {
                currentState = PinkPlayerState.Stun;
            }
        }

        Debug.Log(photonView.ViewID + " �÷��̾� ü�� ������Ʈ��: " + runTimeData.currentHealth);
    }

    // ����

    // GaugeInteraction Ŭ���� ����
    private GaugeInteraction gaugeInteraction;

    private Coroutine stunCoroutine;
    private void EnterStunState()
    {
        currentState = PinkPlayerState.Stun;
        Debug.Log("�÷��̾� ����");
        animator.SetBool("stun", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "stun", true);
        }

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(CoStunDuration());
    }

    private IEnumerator CoStunDuration()
    {
        float stunDuration = 30f;
        float elapsed = 0f;

        if (photonView.IsMine)
        {
            stunOverlay.enabled = true;
            stunSlider.gameObject.SetActive(true);
            stunSlider.fillAmount = 1f;
            hpBar.enabled = false;  // ���� ���¿��� ü�¹� ��Ȱ��ȭ
        }

        while (elapsed < stunDuration && currentState == PinkPlayerState.Stun)
        {
            elapsed += Time.deltaTime;

            if (photonView.IsMine)
            {
                stunSlider.fillAmount = 1 - (elapsed / stunDuration);
            }

            yield return null;
        }

        if (currentState == PinkPlayerState.Stun)  // ������ �������¶��
        {
            TransitionToDeath();
        }

        if (photonView.IsMine)
        {
            stunSlider.gameObject.SetActive(false);
            stunOverlay.enabled = false;
        }
    }

    public void Revive()
    {
        if (!photonView.IsMine)
        {
            photonView.RPC("ReviveRPC", photonView.Owner);
        }

        if (currentState == PinkPlayerState.Stun)
        {
            if (stunCoroutine != null)
            {
                StopCoroutine(stunCoroutine);
                stunCoroutine = null;
            }

            currentState = PinkPlayerState.Idle;

            if (photonView.IsMine)
            {
                stunSlider.gameObject.SetActive(false);
                stunOverlay.enabled = false;
                hpBar.enabled = true;
            }

            animator.SetBool("revive", true);
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("SyncBoolParameter", RpcTarget.Others, "revive", true);
            }

            photonView.RPC("UpdateHP", RpcTarget.All, 20f); // ���⼭ ü�� ������Ʈ
            Debug.Log("�÷��̾� ��Ȱ");
        }
    }

    // RPC
    [PunRPC]
    public void ReviveRPC()
    {
        Revive();
    }

    private void TransitionToDeath()
    {
        currentState = PinkPlayerState.Death;
        Debug.Log("�÷��̾� ���");
        if (photonView.IsMine)
        {
            stunSlider.gameObject.SetActive(false);
            stunOverlay.enabled = false;
            hpBar.enabled = false;  // ����� ü�¹� ��Ȱ��ȭ
        }

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
    public override void SyncBoolParameter(string parameter, bool value)
    {
        base.SyncBoolParameter(parameter, value);
    }

    public override void SetBoolParameter(string parameter, bool value)
    {
        base.SetBoolParameter(parameter, value);
    }

    [PunRPC]
    public override void SyncIntParameter(string parameter, int value)
    {
        base.SyncIntParameter(parameter, value);
    }

    public override void SetIntParameter(string parameter, int value)
    {
        base.SetIntParameter(parameter, value);
    }

    public override void RecoverHealth(float value)
    {
        base.RecoverHealth(value);
    }

    public override void AddShield(float amount, float duration)
    {
        base.AddShield(amount, duration);
    }

    public override void UpdateBlessingRunTimeData(SkillWithLevel newData)
    {
        base.UpdateBlessingRunTimeData(newData);

        if (newData.level == 1 && newData.skillData.Bind_Key == (int)Skills.Mouse_R)
        {
            animator.SetInteger("mouseRightBlessing", newData.skillData.Devil);
        }
    }

    public void Guard_01_Crocell_AddShield()
    {
        playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this).ApplyEffect();
    }
}
