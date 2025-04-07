using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;


public enum WhitePlayerState { Idle, Run, BasicAttack, Hit, Dash, Skill, Ultimate, Guard, Parry, Counter, Stun, Revive, Death }

public class WhitePlayerController : ParentPlayerController
{
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

    protected override void Awake()

    {
        //AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();

        base.Awake();
    }

    private void Start()
    {
        currentState = WhitePlayerState.Idle;

        if (photonView.IsMine)
        {
            if (photonView.IsMine)
            {

                if (stunOverlay != null) stunOverlay.enabled = false;
                if (stunSlider != null) stunSlider.enabled = false;
                if (hpBar != null) hpBar.enabled = true;

                gaugeInteraction = GetComponentInChildren<GaugeInteraction>();

                var eventController = GetComponent<WhitePlayercontroller_event>();
                if (eventController != null)
                {
                    //eventController.OnInteractionEvent += HandleReviveInteraction;
                }
            }
        }
    }

    private void Update()
    {
        if (currentState == WhitePlayerState.Death)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                RoomManager.Instance.SwitchCameraToNextPlayer();
            }
            return;
        }

        UpdateCenterPoint();
        HandleMovement();
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

    // �뽬 ó��

    public void HandleDash()
    {
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Dash)
            return;
        if (!cooldownCheckers[(int)Skills.Space].CanUse())
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
            else if (currentState == WhitePlayerState.Counter && animator.GetInteger("CounterStack") > 0)
            {
                animator.SetBool("Counter", true);
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Counter", true);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
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
            if (cooldownCheckers[(int)Skills.Shift_L].CanUse() && nextState < WhitePlayerState.Skill)
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
            if (cooldownCheckers[(int)Skills.R].CanUse() && nextState < WhitePlayerState.Ultimate)
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

    #region ��ų ����Ʈ ����

    // �ñر� ����Ʈ ����
    public void CreateUltimateEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}", transform.position + new Vector3(8.5f, 0, 0), Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}"), transform.position + new Vector3(8.5f, 0, 0), Quaternion.identity);
                skillEffect.Init(damage, StartHitlag);
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}", transform.position + new Vector3(-8.5f, 0, 0), Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}"), transform.position + new Vector3(-8.5f, 0, 0), Quaternion.identity);
                skillEffect.Init(damage, StartHitlag);
            }
        }
    }

    // ��Ÿ ����Ʈ ����
    public void CreateBasicAttackEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                    float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Attack{attackStack}_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Attack{attackStack}_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
                skillEffect.transform.parent = transform;
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                    float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Attack{attackStack}_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
                }
            }
            else
            {
                float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Attack{attackStack}_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
            }
        }
    }

    // ����Ʈ ��ų ����Ʈ ����
    public void CreateShiftSkillEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/ShiftSkill_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/ShiftSkill_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));
                skillEffect.transform.parent = transform;
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/ShiftSkill_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/ShiftSkill_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));
            }
        }
    }

    // ī���� ����Ʈ ����
    public void CreateCounterSkillEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Counter_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Counter_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.Init(damage, StartHitlag);
                skillEffect.transform.parent = transform;
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Counter_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Counter_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag);
            }
        }
    }

    // �и� ����Ʈ ����
    public void CreateParrySkillEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Parry_Right_Effect", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Parry_Right_Effect"), transform.position, Quaternion.identity);
                skillEffect.Init(damage, StartHitlag);
                skillEffect.transform.parent = transform;
            }
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/WhitePlayer/Parry_Left_Effect", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/WhitePlayer/Parry_Left_Effect"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag);
            }
        }
    }

    // �����̽� ����Ʈ �����̳ʿ� ȿ���� ��Ÿ������
    public void CreateSpaceSkillEffect()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (photonView.IsMine)
            {
                SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/EffectContainer", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                skillEffect.transform.parent = transform;
                skillEffect.Init(0, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Space].skillData.ID, this));
            }
        }
        else
        {
            SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/EffectContainer"), transform.position, Quaternion.identity);
            skillEffect.transform.parent = transform;
            skillEffect.Init(0, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Space].skillData.ID, this));
        }
    }

    #endregion

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

    // ����/�и� ó��
    public void HandleGuard()
    {
        if (currentState != WhitePlayerState.Death)
        {
            if (cooldownCheckers[(int)Skills.Mouse_R].CanUse() && nextState < WhitePlayerState.Guard)
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
                cooldownCheckers[(int)Skills.Mouse_R].ResetCooldown(this);
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

        if (runTimeData.currentHealth <= 0)
        {
            if (currentState != WhitePlayerState.Stun)
            {
                currentState = WhitePlayerState.Stun;
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
        currentState = WhitePlayerState.Stun;
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
            stunSlider.enabled = true;
            stunSlider.fillAmount = 1f;
            hpBar.enabled = false;  // ���� ���¿��� ü�¹� ��Ȱ��ȭ
        }

        while (elapsed < stunDuration && currentState == WhitePlayerState.Stun)
        {
            elapsed += Time.deltaTime;

            if (photonView.IsMine)
            {
                stunSlider.fillAmount = 1 - (elapsed / stunDuration);
            }

            yield return null;
        }

        if (currentState == WhitePlayerState.Stun)  // ������ �������¶��
        {
            TransitionToDeath();
        }

        if (photonView.IsMine)
        {
            stunSlider.enabled = false;
            stunOverlay.enabled = false;
        }
    }

    public void Revive()
    {
        if (!photonView.IsMine)
        {
            photonView.RPC("ReviveRPC", photonView.Owner);
        }

        if (currentState == WhitePlayerState.Stun)
        {
            if (stunCoroutine != null)
            {
                StopCoroutine(stunCoroutine);
                stunCoroutine = null;
            }

            currentState = WhitePlayerState.Idle;

            if (photonView.IsMine)
            {
                stunSlider.enabled = false;
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
        currentState = WhitePlayerState.Death;
        Debug.Log("�÷��̾� ���");
        if (photonView.IsMine)
        {
            stunSlider.enabled = false;
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
