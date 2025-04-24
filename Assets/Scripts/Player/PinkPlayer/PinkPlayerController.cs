using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;

public enum PinkPlayerState { Idle, Run, tackle ,BasicAttack, Hit, Dash, Skill, Ultimate, R_Idle, R_hit1, R_hit2, R_hit3, R_finish, Charge1, Charge2, Charge3, Stun, Revive, Death }

public class PinkPlayerController : ParentPlayerController
{
    [Header("대쉬 설정")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    //private float lastDashClickTime = -Mathf.Infinity;

    [Header("중심점 설정")]
    [Tooltip("기본 CenterPoint (애니메이션 이벤트 등에서 사용)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8방향 CenterPoint 배열 (순서: 0=위, 1=우상, 2=오른쪽, 3=우하, 4=아래, 5=좌하, 6=왼쪽, 7=좌상)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    [Header("서번트 소환 설정")]
    [SerializeField] private GameObject servantPrefab;             
    [SerializeField] private Vector3 servantSpawnOffset = new Vector3(0f, 0.5f, 0f);

    // 이동 입력 및 상태
    private Vector2 moveInput;
    public PinkPlayerState currentState = PinkPlayerState.Idle;
    public PinkPlayerState nextState = PinkPlayerState.Idle;

    protected override void Awake()

    {

        base.Awake();
    }

    private void Start()
    {
        currentState = PinkPlayerState.Idle;

        if (photonView.IsMine)
        {
            if (photonView.IsMine)
            {

                if (stunOverlay != null) stunOverlay.enabled = false;
                if (stunSlider != null) stunSlider.enabled = false;
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

    // 입력 처리 관련
    // WhitePlayercontroller_event.cs에서 호출하여 이동 입력을 설정
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    // 이동 처리
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


        if (currentState != PinkPlayerState.Run)
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

    // 대쉬 처리

    public void HandleDash()
    {
        if (currentState == PinkPlayerState.Death || currentState == PinkPlayerState.Dash)
            return;
        if (currentState == PinkPlayerState.Stun)
            return;
        if (!cooldownCheckers[(int)Skills.Space].CanUse())
            return;
        currentState = PinkPlayerState.Dash;
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

    // 평타
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

            //else if (currentState == PinkPlayerState.R_hit1 && animator.GetInteger("attackStack") > 1)
            //{
            //    animator.SetBool("basicattack", true);
            //    Vector3 mousePos = GetMouseWorldPosition();
            //    animator.SetBool("Right", mousePos.x > transform.position.x);
            //    if (PhotonNetwork.IsConnected)
            //    {
            //        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "basicattack", true);
            //        photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
            //    }
            //    currentState = PinkPlayerState.R_hit2;
            //    return;
            //}

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

            if (nextState == PinkPlayerState.Idle)
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                }
                currentState = PinkPlayerState.BasicAttack;
                nextState = PinkPlayerState.BasicAttack;
            }

            if(attackStack == 2)
            {

            }

            if (attackStack >= 2)
            {
                animator.SetBool("Pre-Attack", false);
                animator.SetBool("Pre-Input", false);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", false);
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
                }
                currentState = PinkPlayerState.Idle;
                attackStack = 0;
                AttackStackUpdate?.Invoke(attackStack);
                Debug.Log("공격 스택 2 도달: 콤보 종료 및 초기화");
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

    // 우클릭

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

    // 차지

    private bool isCharging = false;
    private int chargeLevel = 0;

    public float physicalAtk;
    public float magicAtk;

    // 우클릭 눌렀을 때 (차지 시작)
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

    // 우클릭을 놓았을 때 (차지 종료)
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


    // 프레임별 차지 레벨 설정 (애니메이션 이벤트로 호출)
    public void SetChargeLevel(int level)
    {
        chargeLevel = level;
        animator.SetInteger("chargeLevel", level); // 애니메이터 파라미터 업데이트

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

        Debug.Log($"차지 레벨: {chargeLevel}, nextState: {nextState}");
    }





    // 특수 공격
    public void HandleSpecialAttack()
    {
        if (currentState != PinkPlayerState.Death)
        {
            if (cooldownCheckers[(int)Skills.Shift_L].CanUse() && nextState < PinkPlayerState.Skill)
            {
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
                    // 소환수 소환 RPC 호출
                    photonView.RPC("RPC_SpawnServant", RpcTarget.All);
                }
            }

        }
    }

    // 실제 소환수 소환 로직 분리
    private void SpawnServant()
    {
        Vector3 spawnPos = transform.position + servantSpawnOffset;
        Instantiate(servantPrefab, spawnPos, Quaternion.identity);
    }

    // PUN RPC: 모든 클라이언트에서 호출되어 로컬 Instantiate 수행
    [PunRPC]
    private void RPC_SpawnServant()
    {
        SpawnServant();
    }

    private int servantCount = 0;
    private const int maxUltimateStacks = 8;


    // 궁극기

    /// <summary>
    /// R 키 입력이 들어왔을 때,
    /// 현재 상태에 따라 궁극기 시작 or 마지막 일격 실행
    /// </summary>
    public void OnUltimateInput(InputAction.CallbackContext context)
    {
        if (animator == null || currentState == PinkPlayerState.Death)
            return;
        // 2) 키가 눌린 순간(started)만
        if (context.started)
        {
            currentState = PinkPlayerState.R_Idle;
            // R_idle 애니메이션 진입용 Bool/Trigger
            animator.SetBool("ultimate", true);
        }
    }

    // 궁극기 시전 시작
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

    // 궁극기 지속시간이 다 되거나 R을 다시 누를 때 호출
    public void HandleUltimateEndOrFinal()
    {
        // 애니메이터에서 마지막 일격(FinalStrike) 상태로 전이
        currentState = PinkPlayerState.Ultimate;
        animator.SetTrigger("FinalStrike");
        animator.SetTrigger("FinalStrike");
        if (PhotonNetwork.IsConnected)
            photonView.RPC("PlayAnimation", RpcTarget.Others, "FinalStrike");
    }

    [PunRPC]
    public void RPC_SpawnServant_Stack()
    {
        SpawnServant();
        // 스택 갱신
        servantCount = Mathf.Min(servantCount + 1, maxUltimateStacks);
        Debug.Log($"서번트 소환! 현재 스택: {servantCount}");
    }

    // 애니메이션 이벤트로 평타 스택 설정해주기
    public void OnAttackStack(int stack)
    {
        if (currentState != PinkPlayerState.BasicAttack) return;

        attackStack = stack;
        AttackStackUpdate?.Invoke(attackStack);
        Debug.Log($"평타 스택 설정: {attackStack}");
    }

    // 쉴드 관련 이벤트로 호출
    public void OnUltimateCastComplete()
    {
        int stacks = servantCount;
        float totalShield = 30f * stacks;
        float totalDuration = 2f * stacks;

        if (stacks > 0)
        {
            AddShield(totalShield, totalDuration);
            Debug.Log($"궁극기 쉴드: +{totalShield}HP, 지속 {totalDuration}s (스택 {stacks})");
            servantCount = 0;   // 스택 초기화
        }
    }


    //public WhitePlayerAttackZone AttackCollider;

    // 공격 애니메이션 이벤트용 스텁 (WhitePlayerController_AttackStack에서 호출) 

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

    // 애니메이션 이벤트 함수

    public void OnAttackPreAttckStart()
    {
        animator.SetBool("CancleState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        }
        Debug.Log("선딜 시작");
    }

    public void OnAttackPreAttckEnd()
    {
        animator.SetBool("CancleState", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        }
        Debug.Log("선딜 종료");
    }

    public void OnAttackLastAttckStart()
    {
        animator.SetBool("CancleState2", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", true);
        }
        Debug.Log("후딜 시작");
    }

    public void OnAttackLastAttckEnd()
    {
        animator.SetBool("CancleState2", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", false);
        }
        Debug.Log("후딜 종료");
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


    #region 스킬 이펙트 생성

    // 궁극기 이펙트 생성
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

    // 평타 이펙트 생성
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
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_attack{attackStack}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_attack{attackStack}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}"), transform.position, Quaternion.identity);
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
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_attack{attackStack}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
                }
            }
            else
            {
                float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
                float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_attack{attackStack}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));
            }
        }
    }

    // 차징 이펙트 생성
    public void CreateChargeSkillEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
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
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_charging{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag);
            }
        }
    }

    // 차징 홀드 해제 후 이펙트 생성

    public void CreateChargeHItSkillEffect()
    {
        if (animator.GetBool("Right"))
        {
            if (PhotonNetwork.IsConnected)
            {
                if (photonView.IsMine)
                {
                    float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.Init(damage, StartHitlag);
                    skillEffect.transform.parent = transform;
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
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
                    SkillEffect skillEffect = PhotonNetwork.Instantiate($"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}", transform.position, Quaternion.identity).GetComponent<SkillEffect>();
                    skillEffect.transform.parent = transform;
                    skillEffect.Init(damage, StartHitlag);
                }
            }
            else
            {
                float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower + runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;
                SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>($"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}"), transform.position, Quaternion.identity);
                skillEffect.transform.parent = transform;
                skillEffect.Init(damage, StartHitlag);
            }
        }
    }

    // 시프트 스킬 이펙트 생성
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


    // 패링 이펙트 생성
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

    // 스페이스 이펙트 컨테이너에 효과만 나타나도록
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
        Debug.Log("궁극기 납도 버프");
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
        Debug.Log("자유상태");
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
        Debug.Log(" 애니메이션 종료");
    }

    public void InitAttackStak()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }


    // 피격 및 사망 처리
    public override void TakeDamage(float damage)
    {
        if (currentState == PinkPlayerState.Death || currentState == PinkPlayerState.Stun)
        {
            return;
        }
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

        base.TakeDamage(damage);

        Debug.Log("플레이어 체력: " + runTimeData.currentHealth);

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
            if (currentState != PinkPlayerState.Stun)
            {
                currentState = PinkPlayerState.Stun;
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
        currentState = PinkPlayerState.Stun;
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

        while (elapsed < stunDuration && currentState == PinkPlayerState.Stun)
        {
            elapsed += Time.deltaTime;

            if (photonView.IsMine)
            {
                stunSlider.fillAmount = 1 - (elapsed / stunDuration);
            }

            yield return null;
        }

        if (currentState == PinkPlayerState.Stun)  // 여전히 기절상태라면
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
        currentState = PinkPlayerState.Death;
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
