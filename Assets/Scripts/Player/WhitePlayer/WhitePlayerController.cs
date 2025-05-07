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
    [Header("대쉬 설정")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    private Vector3 dashDirection;
    //private float lastDashClickTime = -Mathf.Infinity;

    [Header("중심점 설정")]
    [Tooltip("기본 CenterPoint (애니메이션 이벤트 등에서 사용)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8방향 CenterPoint 배열 (순서: 0=위, 1=우상, 2=오른쪽, 3=우하, 4=아래, 5=좌하, 6=왼쪽, 7=좌상)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    // 이동 입력 및 상태
    private Vector2 moveInput;
    public WhitePlayerState currentState = WhitePlayerState.Idle;
    public WhitePlayerState nextState = WhitePlayerState.Idle;

    protected override void Awake()

    {
        //AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        currentState = WhitePlayerState.Idle;

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
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

    private void Update()
    {
        //if (currentState == WhitePlayerState.Death)
        //{
        //    if (Input.GetKeyDown(KeyCode.X))
        //    {
        //        RoomManager.Instance.SwitchCameraToNextPlayer();
        //    }
        //    return;
        //}

        //UpdateCenterPoint();
        //Handle    Movement();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

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

    // 입력 처리 관련
    // WhitePlayercontroller_event.cs에서 호출하여 이동 입력을 설정
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
        // x축 입력이 있을 때만 마지막 바라본 방향 갱신
        if (Mathf.Abs(input.x) > 0.01f)
            lastFacingDirection = new Vector3(Mathf.Sign(input.x), 0f, 0f);
    }

    // 이동 처리
    private void HandleMovement()
    {
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Stun)
        {
            return;
        }

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
            rb.MovePosition(rb.position + moveDir * runTimeData.moveSpeed * Time.fixedDeltaTime);
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

    private Vector3 lastFacingDirection = Vector3.right;




    // 대쉬 처리
    public void HandleDash()
    {
        // 1) 상태 체크
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Dash)
            return;
        if (currentState == WhitePlayerState.Stun)
            return;
        if (!cooldownCheckers[(int)Skills.Space].CanUse())
            return;

        // 2) 상태 전환 & 애니메이터
        currentState = WhitePlayerState.Dash;
        animator.ResetTrigger("run");
        animator.SetBool("dash", true);
        if (PhotonNetwork.IsConnected)
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "dash", true);

        // 3) 대쉬 방향 결정
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            // 입력이 있을 때
            dashDirection = new Vector3(Mathf.Sign(moveInput.x), 0f, 0f);
        }
        else
        {
            // 입력 없을 땐 마지막 바라본 방향 사용
            dashDirection = lastFacingDirection;
        }

        // 4) 실제 이동 코루틴 호출 (필요에 따라 주석 해제)
        // StartCoroutine(DoDash(dashDirection));
    }

    //private IEnumerator DoDash(Vector3 dashDir)
    //{
    //    Vector3 startPos = transform.position;
    //    Vector3 targetPos = startPos + dashDir.normalized * dashDistance;
    //    yield return null;
    //    transform.position = targetPos;
    //}


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
            else if (nextState < WhitePlayerState.BasicAttack && cooldownCheckers[(int)Skills.Mouse_L].CanUse())
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
                nextState = WhitePlayerState.BasicAttack;
            }

            if (currentState == WhitePlayerState.BasicAttack && cooldownCheckers[(int)Skills.Mouse_L].CanUse())
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

    // 특수 공격
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

    // 궁극기 공격 
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

    // 공격 애니메이션 이벤트용 스텁 (WhitePlayerController_AttackStack에서 호출) 

    public override void StartMouseRCoolDown()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.StartMouseRCoolDown();
    }

    public override void StartShiftCoolDown()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.StartShiftCoolDown();
    }

    public override void StartUltimateCoolDown()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.StartUltimateCoolDown();
    }

    public override void StartAttackCooldown()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.StartAttackCooldown();
    }

    public override void StartSpaceCooldown()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.StartSpaceCooldown();
    }

    public void OnAttackPreAttckStart()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        animator.SetBool("CancleState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        }
        Debug.Log("선딜 시작");
    }

    public void OnAttackPreAttckEnd()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        animator.SetBool("CancleState", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        }
        Debug.Log("선딜 종료");
    }

    public void OnMoveFront(float value)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        //transform.Translate((GetMouseWorldPosition() - transform.position).normalized * value);
        rb.MovePosition(rb.position + ((GetMouseWorldPosition() - transform.position).normalized * value));
    }

    public void OnMoveFront2(float value)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        Vector3 movement = dashDirection * value;
        rb.MovePosition(rb.position + movement);
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

    #region 스킬 이펙트 생성

    // 궁극기 이펙트 생성
    public void CreateUltimateEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = null;
        Vector3 targetPos = transform.position;
        string effectPath;
        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos + new Vector3(6f, 0, 0), Quaternion.identity);
        }
        else
        {
            effectPath = $"SkillEffect/WhitePlayer/WhitePlayer_Ultimateffect_Left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos + new Vector3(-6f, 0, 0), Quaternion.identity);
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos);

        skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine, isMine ? playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.R].skillData.ID, this) : null);
    }

    // 평타 이펙트 생성
    public void CreateBasicAttackEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
        float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient * damageBuff;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = null;
        Vector3 targetPos = transform.position;
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/WhitePlayer/Attack{attackStack}_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);
        }
        else
        {
            effectPath = $"SkillEffect/WhitePlayer/Attack{attackStack}_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos);

        skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine, isMine ? playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this) : null);
        skillEffect.transform.parent = transform;
    }

    // 시프트 스킬 이펙트 생성
    public void CreateShiftSkillEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float damage = (runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = null;
        Vector3 targetPos = transform.position;
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/WhitePlayer/ShiftSkill_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }
        else
        {
            effectPath = $"SkillEffect/WhitePlayer/ShiftSkill_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos);

        skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine, isMine ? playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this) : null);
        skillEffect.transform.parent = transform;
    }

    // 카운터 이펙트 생성
    public void CreateCounterSkillEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = null;
        Vector3 targetPos = transform.position;
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/WhitePlayer/Counter_Right_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }
        else
        {
            effectPath = $"SkillEffect/WhitePlayer/Counter_Left_Effect_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos);

        skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine, null);
        skillEffect.transform.parent = transform;
    }

    // 패링 이펙트 생성
    public void CreateParrySkillEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = null;
        Vector3 targetPos = transform.position;
        string effectPath;

        if (animator.GetBool("Right"))
        {
            effectPath = "SkillEffect/WhitePlayer/Parry_Right_Effect";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }
        else
        {
            effectPath = "SkillEffect/WhitePlayer/Parry_Left_Effect";
            skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), transform.position, Quaternion.identity);
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos);

        skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine, null);
        skillEffect.transform.parent = transform;
    }

    // 스페이스 이펙트 컨테이너에 효과만 나타나도록
    public void CreateSpaceSkillEffect()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Photon에 접속 중이 아닐 때 photonView.IsMine 값은 false로 처리
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/EffectContainer"), transform.position, Quaternion.identity);
        skillEffect.transform.parent = transform;
        skillEffect.Init(0, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Space].skillData.ID, this));
    }

    #endregion

    [PunRPC]
    public override void CreateAnimation(string name, Vector3 pos)
    {
        base.CreateAnimation(name, pos);
    }

    public void GetUltimateBonus()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        Debug.Log("궁극기 납도 버프");
    }

    public void UltimateMove(float distance)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        //transform.position += new Vector3(distance, 0, 0);
        Vector3 targetPosition = rb.position + new Vector3(distance, 0, 0);
        rb.MovePosition(targetPosition);
    }

    public void OnAttackAllowNextInput()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        animator.SetBool("FreeState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", true);
        }
        Debug.Log("자유상태");
    }

    public void OnAttackAnimationEnd()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

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
        Debug.Log(" 애니메이션 종료");
    }

    public void InitAttackStak()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }

    // 가드/패링 처리
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

    // 피격 및 사망 처리
    public override void TakeDamage(float damage, AttackerType attackerType = AttackerType.Default)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }
        if (currentState == WhitePlayerState.Death || currentState == WhitePlayerState.Stun)
        {
            return;
        }
        AudioManager.Instance.PlayOneShot("event:/Character/Common/Character Hit", transform.position);
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

        Debug.Log("플레이어 체력: " + runTimeData.currentHealth);

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
        Debug.Log(photonView.ViewID + "플레이어 체력: " + runTimeData.currentHealth);

        if (runTimeData.currentHealth <= 0)
        {
            if (currentState != WhitePlayerState.Stun)
            {
                currentState = WhitePlayerState.Stun;
            }
        }

        Debug.Log(photonView.ViewID + " 플레이어 체력 업데이트됨: " + runTimeData.currentHealth);
    }

    // 기절

    // GaugeInteraction 클래스 참조
    private GaugeInteraction gaugeInteraction;

    private Coroutine stunCoroutine;
    private void EnterStunState()
    {
        currentState = WhitePlayerState.Stun;
        Debug.Log("플레이어 기절");
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
            hpBar.enabled = false;  // 기절 상태에선 체력바 비활성화
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

        if (currentState == WhitePlayerState.Stun)  // 여전히 기절상태라면
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
        Debug.Log("Revive 실행됨");

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

            photonView.RPC("UpdateHP", RpcTarget.All, 20f); // 여기서 체력 업데이트
            Debug.Log("플레이어 부활");
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
        Debug.Log("플레이어 사망");
        if (photonView.IsMine)
        {
            stunSlider.enabled = false;
            stunOverlay.enabled = false;
            hpBar.enabled = false;  // 사망시 체력바 비활성화
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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.EnterInvincibleState();
    }

    public override void ExitInvincibleState()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.ExitInvincibleState();
    }

    public override void EnterSuperArmorState()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        base.EnterSuperArmorState();
    }

    public override void ExitSuperArmorState()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this).ApplyEffect();
    }

    public override void ShadowOff()
    {
        base.ShadowOff();
    }

    public override void ShadowOn()
    {
        base.ShadowOn();
    }
}
