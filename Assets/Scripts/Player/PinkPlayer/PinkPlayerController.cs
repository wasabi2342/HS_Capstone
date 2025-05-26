using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

public enum PinkPlayerState { Idle, R_Idle, Run, tackle, BasicAttack, Hit, Dash, Skill, Ultimate, R_hit, Charge1, Charge2, Charge3, R_finish, Stun, Revive, Death }

public class PinkPlayerController : ParentPlayerController
{
    public PlayerRunTimeData RuntimeData => runTimeData;

    [Header("대쉬 설정")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    public float dashDuration = 0.2f;
    private Vector3 dashDirection;
    private Vector3 facingDirection = Vector3.right;
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
    [SerializeField] private GameObject timeServantPrefab; // 이는 시간 가호 쉬프트를 먹었을 때 (죽음의 망자들) 사용 될 프리팹
    [SerializeField] private Vector3 servantSpawnOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private int DefaultMaxServants = 8;
    // 죽이기 전 불러온 소환수 개수를 저장
    private int ultimateStackLimit = 0;
    private const int TimeMaxServants = 13; // 시간 가호 소환수 최대 개체 수 

    private int CurrentMaxServants =>
    runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil == 2
    ? TimeMaxServants
    : DefaultMaxServants;

    private List<ServantFSM> summonedServants = new List<ServantFSM>();
    [Header("궁극기 설정")]
    [SerializeField] private float ultimateDuration = 5f;
    public bool isUltimateActive = false;

    // 이동 입력 및 상태
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

                //if (runTimeData.currentHealth <= 0)
                //{
                //    TransitionToDeath();
                //}

            }
        }
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

    // 입력 처리 관련

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
                    //if (PhotonNetwork.IsConnected)
                    //{
                    //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                    //}
                }
            }
            else if (nextState > PinkPlayerState.Run)
            {
                if (animator.GetBool("run"))
                {
                    animator.SetBool("run", false);
                    if (PhotonNetwork.IsConnected)
                        SetBoolParameter("run", false);
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
                if (PhotonNetwork.IsConnected)
                    SetBoolParameter("run", false);
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
            // 여기서만 방향 고정 갱신
            if (h > 0.01f) facingDirection = Vector3.right;
            else if (h < -0.01f) facingDirection = Vector3.left;

            // 이동
            Vector3 moveDir = (Mathf.Abs(v) > 0.01f)
                ? new Vector3(h, 0, v).normalized
                : new Vector3(h, 0, 0).normalized;
            rb.MovePosition(rb.position + moveDir * runTimeData.moveSpeed * Time.fixedDeltaTime);

            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
            SetFloatParameter("moveX", h);
            SetFloatParameter("moveY", v);
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



    // 대쉬 처리

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

        // 여기서는 무조건 facingDirection 사용
        dashDirection = facingDirection;

        //StartCoroutine(DoDash());
    }


    // 평타
    public void HandleNormalAttack()
    {

        if (currentState != PinkPlayerState.Death || currentState != PinkPlayerState.Stun)
        {

            if (currentState == PinkPlayerState.R_Idle)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                //animator.SetBool("basicattack", true);
                //if (PhotonNetwork.IsConnected)
                //{
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "basicattack", true);
                //}
                //photonView.RPC("PlayAnimation", RpcTarget.All, "basicattack");
                nextState = PinkPlayerState.R_hit;

            }



            if (currentState == PinkPlayerState.R_hit)
            {
                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("Pre-Input", true);
                nextState = PinkPlayerState.R_hit;

            }

            if (nextState < PinkPlayerState.BasicAttack && nextState != PinkPlayerState.R_Idle && cooldownCheckers[(int)Skills.Mouse_L].CanUse())
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("Pre-Input", true);
                //if (PhotonNetwork.IsConnected)
                //{
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                //}
                nextState = PinkPlayerState.BasicAttack;
            }


            //if (attackStack > 2)
            //{
            //    animator.SetBool("Pre-Attack", false);
            //    animator.SetBool("Pre-Input", false);
            //    //if (PhotonNetwork.IsConnected)
            //    //{
            //    //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", false);
            //    //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
            //    //}
            //    //currentState = PinkPlayerState.Idle;
            //    //attackStack = 0;
            //    AttackStackUpdate?.Invoke(attackStack);
            //    Debug.Log("공격 스택 2 도달: 콤보 종료 및 초기화");
            //    return;
            //}

            if (currentState == PinkPlayerState.BasicAttack && cooldownCheckers[(int)Skills.Mouse_L].CanUse())
            {

                Vector3 mousePos = GetMouseWorldPosition();
                animator.SetBool("Right", mousePos.x > transform.position.x);
                animator.SetBool("Pre-Input", true);

                //if (PhotonNetwork.IsConnected)
                //{
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Right", mousePos.x > transform.position.x);
                //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Input", true);
                //}
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
                if (myServants.Count >= CurrentMaxServants)
                {
                    Debug.Log($"최대 소환수 개수({CurrentMaxServants})에 도달함.");
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
                    photonView.RPC("RPC_SpawnServant", RpcTarget.MasterClient, photonView.ViewID, runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil);
                }
            }

        }
    }

    // 실제 소환수 소환 로직 분리
    [PunRPC]
    private void RPC_SpawnServant(int ownerViewID, int blessingIndex)
    {

        int devilLevel = blessingIndex;
        // 시간가호->  TimeServant, 그 외엔 기본 Servant
        string prefabName = (devilLevel == 2)
        ? timeServantPrefab.name    // 인스펙터에 할당된 TimeServant 프리팹의 이름
        : servantPrefab.name;       // 기존 Servant 프리팹의 이름

        Debug.Log($"[RPC_SpawnServant] devilLevel={devilLevel} → Spawn: {prefabName}");


        Vector3 spawnPos = transform.position + servantSpawnOffset;
        GameObject servant = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);
        ServantFSM servantFSM = servant.GetComponent<ServantFSM>();

        if (blessingIndex == 1)
        {
            Debug.Log("소환수 도발!");
            servantFSM.TauntEnemy(30f); // 확실히 알려고 30초 때려박음 --> 원래 3초
        }

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
    /// <summary>ServantFSM 인스턴스를 리스트에 등록합니다.</summary>
    public void AddServantToList(ServantFSM servant)
    {
        if (servant != null && !summonedServants.Contains(servant))
            summonedServants.Add(servant);
    }

    /// <summary>ServantFSM 인스턴스를 리스트에서 해제합니다.</summary>
    public void RemoveServantFromList(ServantFSM servant)
    {
        if (servant != null)
            summonedServants.Remove(servant);
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
        if (!photonView.IsMine) return;
        // 버튼 눌린 시점에만
        if (context.started && !isUltimateActive && cooldownCheckers[(int)Skills.R].CanUse())
        {
            // MasterClient 권한으로 실제 궁극기 실행
            photonView.RPC(nameof(RPC_UseUltimate), RpcTarget.MasterClient);

            // 로컬 상태 세팅
            R_attackStack = 0;
            nextState = PinkPlayerState.Ultimate;
            animator.SetBool("ultimate", true);


            Vector3 mousePos = GetMouseWorldPosition();
            bool isRight = mousePos.x > transform.position.x;
            animator.SetBool("Right", isRight);

        }

        if (isUltimateActive)
        {
            Vector3 mousePos = GetMouseWorldPosition();
            bool isRight = mousePos.x > transform.position.x;
            animator.SetBool("Right", isRight);

            nextState = PinkPlayerState.R_finish;
        }
            
        //if (context.started)
        //{
        //    // 로컬 상태 세팅
        //    R_attackStack = 0;
        //    currentState = PinkPlayerState.R_Idle;
        //    animator.SetBool("ultimate", true);


        //    Vector3 mousePos = GetMouseWorldPosition();
        //    bool isRight = mousePos.x > transform.position.x;
        //    animator.SetBool("Right", isRight);


        //    if (PhotonNetwork.IsConnected)
        //    {
        //        photonView.RPC(
        //            "SyncBoolParameter",
        //            RpcTarget.Others,
        //            "ultimate",
        //            true
        //        );
        //        photonView.RPC(
        //            "SyncIntParameter",
        //            RpcTarget.Others,
        //            "R_attackStack",
        //            R_attackStack
        //        );
        //        photonView.RPC(
        //            "SyncBoolParameter",
        //            RpcTarget.Others,
        //            "Right",
        //            isRight
        //        );
        //    }
        //}
    }

    [PunRPC]
    private void RPC_UseUltimate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int myServantsCount = myServants.Count; // 소환수 개수 먼저 로컬에 저장
        int devilLevelR = runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil;// 불러온 소환수 개수 기억 - 같은 값 저장해주는 거임

        // 2) Devil == 2 이면 스택 제한 무제한, 아니면 소환수 개수만큼 제한
        if (devilLevelR == 2)
        {
            ultimateStackLimit = int.MaxValue;
        }
        else
        {
            ultimateStackLimit = myServantsCount;
        }

        // 1) myServants 를 돌며 ForceKill RPC 호출
        foreach (var s in myServants)
        {
            if (s != null && s.photonView != null)
                s.photonView.RPC("ForceKill", RpcTarget.AllBuffered);
        }
        myServants.Clear();

        // 2) (버프 로직은 빈 함수로 두었습니다)
        if (photonView.IsMine)
        {
            RPC_ApplyUltimateBuff(myServantsCount);
        }
        else
        {
            photonView.RPC("RPC_ApplyUltimateBuff", photonView.Owner, myServantsCount);
        }

        // 3) 궁극기 애니/상태 진입
        if (photonView.IsMine)
        {
            currentState = PinkPlayerState.Ultimate;
            animator.SetTrigger("ultimate");
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "ultimate", true);
        }
    }

    /// <summary>
    /// 희생된 소환수 개수에 따른 강화 효과 로직
    /// (현재 빈 함수 - 필요시 여기에 버프 코드를 추가하세요)
    /// </summary>
    [PunRPC]
    private void RPC_ApplyUltimateBuff(int myServantsCount)
    {
        int stacks = myServantsCount;
        float totalShield = 30f * stacks;
        float totalDuration = 2f * stacks;

        if (runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil == 1)
        {
            totalShield = 50f * stacks;
            Debug.Log("쉴드 50으로 부여!");
        }

        if (stacks > 0)
        {
            AddShield(totalShield, totalDuration);
            Debug.Log($"궁극기 쉴드: +{totalShield}HP, 지속 {totalDuration}s (스택 {stacks})");
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
        int servantCount = myServants.Count;
        foreach (var servant in myServants)
        {
            if (servant != null)
            {
                servant.photonView.RPC("ForceKill", RpcTarget.MasterClient);
            }
        }
        myServants.Clear();
        //ApplyUltimateBuff(servantCount);
        // 애니메이터에서 마지막 일격(FinalStrike) 상태로 전이
        currentState = PinkPlayerState.Ultimate;
        animator.SetTrigger("FinalStrike");
        animator.SetTrigger("FinalStrike");
        if (PhotonNetwork.IsConnected)
            photonView.RPC("PlayAnimation", RpcTarget.Others, "FinalStrike");
    }
    //void ApplyUltimateBuff(int servantCount)
    //{
    //    //희생된 소환수 개수에 따른 강화 효과
    //}
    public int R_attackStack;

    // 애니메이션 이벤트로 평타 스택 설정해주기
    public void OnAttackStack()

    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        int devilLevelR = runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil;

        // Devil == 2 이면 무제한으로 계속 증가
        if (devilLevelR == 2)
        {
            R_attackStack++;
        }
        else
        {
            // 기존 cap/limit 로직
            int cap = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.Devil == 2 ? 13 : 8;
            // 최소 3 보장되도록 보정
            int safeLimit = Mathf.Max(2, ultimateStackLimit);
            int maxStack = Mathf.Min(safeLimit, cap);
            if (R_attackStack >= maxStack) return;
            R_attackStack++;
        }

        // 증가 후 동기화
        Debug.Log($"R_attackStack: {R_attackStack}");
        animator.SetInteger("R_attackStack", R_attackStack);
        AttackStackUpdate?.Invoke(R_attackStack);


        SetIntParameter("R_attackStack", R_attackStack);
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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        animator.SetBool("CancleState", true);
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        //}
        Debug.Log("선딜 시작");
    }

    public void OnAttackPreAttckEnd()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        animator.SetBool("CancleState", false);
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        //}
        Debug.Log("선딜 종료");
    }

    public void OnAttackLastAttckStart()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        animator.SetBool("CancleState2", true);
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", true);
        //}
        Debug.Log("후딜 시작");
    }

    public void OnAttackLastAttckEnd()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        animator.SetBool("CancleState2", false);
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState2", false);
        //}
        Debug.Log("후딜 종료");
    }

    public void OnMoveFront(float value)
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
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

    // 핑뚝이 궁극기 초기화 이벤트 함수 -> run일 때 쓸라고 만듦

    public void ResetRAttackStack()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        // 스택 리셋
        R_attackStack = 0;


        animator.SetInteger("R_attackStack", R_attackStack);


        //if (PhotonNetwork.IsConnected && photonView.IsMine)
        //{
        //    photonView.RPC("SyncIntParameter", RpcTarget.Others, "R_attackStack", R_attackStack);
        //}
    }


    #region 스킬 이펙트 생성

    // 궁극기 이펙트 생성
    public void CreateUltimateEffectStart()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // 궁극기 데미지 계산
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;



        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;
        string effectPath;
        // 이펙트 경로 및 위치 설정

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_idle_right_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/Pink_R_idle_left_{runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil}";
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, true, animator.speed);

        // Photon에 접속 중이든 아니든, 로컬에서 이펙트를 생성하는 코드
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // init 메서드 호출
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.R].skillData.ID, this));
        // 궁극기 속도
        skillEffect.SetAttackSpeed(animator.speed);

        // 생성된 이펙트의 부모를 설정
        skillEffect.transform.parent = transform;
    }

    // 궁극기 히트 이펙트 생성
    public void CreateUltimateEffectHit()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // 궁극기 데미지 계산
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * damageBuff;

        Debug.Log($"[R-Effect] 기본 데미지: {damage}");

        // Devil 레벨이 3일 때만 추가 테이블 데미지 적용
        if (runTimeData.skillWithLevel[(int)Skills.R].skillData.Devil == 3)
        {
            int characterId = runTimeData.skillWithLevel[(int)Skills.R].skillData.Character;
            int comboIndex = R_attackStack;  // 현재 스택 수
            float tableDamage = 0f;

            var rCombo = DataManager.Instance.r_AttackComboDatas
                            .Find(x => x.Character == characterId && x.Combo_Index == comboIndex);
            if (rCombo != null)
            {
                tableDamage = rCombo.Damage;
                Debug.Log($"[R-Effect] Table Damage (Char:{characterId}, Combo:{comboIndex}) = {tableDamage}");
            }
            else
            {
                Debug.LogWarning($"[R-Effect] R_AttackComboData 없음 (Char:{characterId}, Combo:{comboIndex})");
            }

            damage += tableDamage;
            Debug.Log($"[R-Effect] 최종 계산된 Damage = {damage}");
        }

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;

        // R_attackStack을 3으로 클램프
        int maxR_attackStackEffect = Mathf.Min(R_attackStack, 2);

        // 이펙트 경로 및 위치 설정
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
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, false, animator.speed);

        var prefab = Resources.Load<SkillEffect>(effectPath);
        if (prefab == null)
        {
            Debug.LogError($"[R-Effect] Prefab 로드 실패! 경로 확인하세요: {effectPath}");
            return;
        }
        Debug.Log($"[R-Effect] Prefab 로드 성공: {prefab.name}");

        // Photon에 접속 중이든 아니든, 로컬에서 이펙트를 생성하는 코드
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // Init 메서드 호출, 여기서 다시 호출할지 말지 고민중 --> 아마 기획 나오면 더 고칠 예정
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        // skillEffect.transform.parent = transform;
    }

    // 궁극기 이펙트 생성 (Finish)
    public void CreateUltimateEffectFinish()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // 궁극기 데미지 계산
        float damage = (runTimeData.skillWithLevel[(int)Skills.R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower);

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 targetPos = transform.position;

        // 이펙트 경로 및 위치 설정
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
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, targetPos, true, animator.speed);

        // Photon에 접속 중이든 아니든, 로컬에서 이펙트를 생성하는 코드
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), targetPos, Quaternion.identity);

        // Init 메서드 호출, 여기서 다시 호출할지 말지 고민중 --> 아마 기획 나오면 더 고칠 예정
        //skillEffect.Init(isMine ? damage : 0, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        skillEffect.transform.parent = transform;
    }

    // 평타 이펙트 생성
    public void CreateBasicAttackEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        if (!photonView.IsMine)
        {
            return;
        }

        // 평타 데미지 계산
        float coefficient = DataManager.Instance.FindDamageByCharacterAndComboIndex(characterBaseStats.characterId, attackStack);
        float damage = (runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                        runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower) * coefficient;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // 이펙트 경로 및 위치 설정
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_attack{attackStack}_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
        }
        else
        {
            effectPath = $"SkillEffect/PinkPlayer/pink_attack{attackStack}_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.Devil}";
            effectPosition = transform.position; // 왼쪽일 때 위치는 기본 위치 그대로 설정
        }

        // 다른 클라이언트에게도 이펙트를 생성하도록 RPC 호출
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true, animator.speed);
        }

        // Photon 연결 여부에 따른 이펙트 생성
        Debug.Log(effectPath);
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init 메서드 호출
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_L].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        skillEffect.transform.parent = transform;
    }

    // 차징 이펙트 생성
    public void CreateChargeSkillEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        if (!photonView.IsMine)
        {
            return;
        }

        // 차징 스킬 데미지 계산
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // 이펙트 경로 및 위치 설정
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

        // 다른 클라이언트에게도 이펙트를 생성하도록 RPC 호출
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true, animator.speed);
        }

        // Photon 연결 여부에 따른 이펙트 생성
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init 메서드 호출
        //skillEffect.Init(damage, StartHitlag, isMine);
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        skillEffect.transform.parent = transform;
    }

    public void CreateChargeHitSkillEffect()
    {
        if (!photonView.IsMine)
            attackStack = animator.GetInteger("AttackStack");

        if (!photonView.IsMine)
        {
            return;
        }

        // 차징 스킬 데미지 계산
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;
        Vector3 effectPosition = transform.position;
        string devil = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil.ToString();

        // ── 1) 기본 Charge Hit 이펙트
        string hitPath = animator.GetBool("Right")
            ? $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_right_{devil}"
            : $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_left_{devil}";

        if (PhotonNetwork.IsConnected && photonView.IsMine)
            photonView.RPC("CreateAnimation", RpcTarget.Others, hitPath, effectPosition, true, animator.speed);

        var hitFx = Instantiate(
            Resources.Load<SkillEffect>(hitPath),
            effectPosition,
            Quaternion.identity
        );
        hitFx.Init(damage, StartHitlag, isMine,
                   playerBlessing.FindSkillEffect(
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID,
                       this
                   ));
        hitFx.transform.parent = transform;

        if (runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil == 3)
        {
            StartCoroutine(DelayedChargeEffect());

            IEnumerator DelayedChargeEffect()
            {
                yield return new WaitForSeconds(1f);


                string chargePath = animator.GetBool("Right")
                     ? $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_right_{devil}"
            : $"SkillEffect/PinkPlayer/pink_charge_hit{chargeLevel}_left_{devil}";

                // y값만 0.1로 고정
                Vector3 spawnPos = new Vector3(
                    effectPosition.x,
                    0.1f,
                    effectPosition.z
                );

                if (PhotonNetwork.IsConnected && photonView.IsMine)
                    photonView.RPC("CreateAnimation", RpcTarget.Others, chargePath, effectPosition, false, animator.speed);

                var chargeFx = Instantiate(
                    Resources.Load<SkillEffect>(chargePath),
                    effectPosition,
                    Quaternion.identity
                );
                chargeFx.Init(damage, StartHitlag, isMine,
                              playerBlessing.FindSkillEffect(
                                  runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID,
                                  this
                              ));
                //chargeFx.transform.parent = transform;
            }
        }
    }


    // 시프트 스킬 이펙트 생성
    public void CreateShiftSkillEffect()
    {
        if (!photonView.IsMine)
        {
            attackStack = animator.GetInteger("AttackStack");
        }

        if (!photonView.IsMine)
        {
            return;
        }

        // 시프트 스킬 데미지 계산
        float damage = runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // 이펙트 경로 및 위치 설정
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

        // 다른 클라이언트에게도 이펙트를 생성하도록 RPC 호출
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, true, animator.speed);
        }

        // Photon 연결 여부에 관계없이 로컬에서 이펙트 생성
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init 메서드 호출
        skillEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Shift_L].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        skillEffect.transform.parent = transform;
    }

    // 태클 이펙트 생성
    public void CreateTackleSkillEffect()
    {

        if (!photonView.IsMine)
        {
            return;
        }

        // 태클 스킬 데미지 계산
        float damage = runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AttackDamageCoefficient * runTimeData.attackPower +
                       runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.AbilityPowerCoefficient * runTimeData.abilityPower;

        // Photon에 접속 중인지 확인하여 isMine 설정
        bool isMine = PhotonNetwork.IsConnected ? photonView.IsMine : true;

        // 이펙트 경로 및 위치 설정
        string damageEffectPath;
        string effectPath;
        Vector3 effectPosition = transform.position;

        if (animator.GetBool("Right"))
        {
            damageEffectPath = $"SkillEffect/PinkPlayer/pink_tackle_left_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
            effectPath = "SkillEffect/PinkPlayer/pink_tackle_back_right_0";
        }
        else
        {
            damageEffectPath = $"SkillEffect/PinkPlayer/pink_tackle_right_{runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.Devil}";
            effectPath = "SkillEffect/PinkPlayer/pink_tackle_back_left_0";
        }

        // 다른 클라이언트에게도 이펙트를 생성하도록 RPC 호출
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            photonView.RPC("CreateAnimation", RpcTarget.Others, damageEffectPath, effectPosition, true, animator.speed);
            photonView.RPC("CreateAnimation", RpcTarget.Others, effectPath, effectPosition, false, animator.speed);
        }

        // Photon 연결 여부에 관계없이 로컬에서 이펙트 생성
        SkillEffect skillDamageEffect = Instantiate(Resources.Load<SkillEffect>(damageEffectPath), effectPosition, Quaternion.identity);
        SkillEffect skillEffect = Instantiate(Resources.Load<SkillEffect>(effectPath), effectPosition, Quaternion.identity);

        // Init 메서드 호출
        //skillEffect.Init(damage, StartHitlag, isMine);
        skillDamageEffect.Init(damage, StartHitlag, isMine, playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this));

        // 생성된 이펙트의 부모를 설정
        skillDamageEffect.transform.parent = transform;
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
    public override void CreateAnimation(string name, Vector3 pos, bool isChild, float speed)
    {
        base.CreateAnimation(name, pos, isChild, speed);
    }

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        animator.SetBool("FreeState", true);
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", true);
        //}
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
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "Pre-Attack", false);
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", false);
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        //}
        Debug.Log(" 애니메이션 종료");
    }

    public void InitAttackStak()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }


    // 피격 및 사망 처리
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
    public override void DamageToMaster(float damage, Vector3 attackerPos)
    {
        base.DamageToMaster(damage, attackerPos);
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
                if (photonView.IsMine)
                    EnterStunState();
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
        //if (PhotonNetwork.IsConnected)
        //{
        //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "stun", true);
        //}

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(CoStunDuration());
    }

    private float stunElapsed;

    private IEnumerator CoStunDuration()
    {
        float stunDuration = 30f;
        stunElapsed = 0f;

        if (photonView.IsMine)
        {
            stunOverlay.enabled = true;
            stunSlider.gameObject.SetActive(true);
            stunSlider.fillAmount = 1f;
            hpBar.enabled = false;  // 기절 상태에선 체력바 비활성화
        }

        while (stunElapsed < stunDuration && currentState == PinkPlayerState.Stun)
        {
            stunElapsed += Time.deltaTime;

            if (photonView.IsMine)
            {
                stunSlider.fillAmount = 1 - (stunElapsed / stunDuration);
            }

            yield return null;
        }

        if (currentState == PinkPlayerState.Stun)  // 여전히 기절상태라면
        {
            TransitionToDeath();
        }

        if (photonView.IsMine)
        {
            stunSlider.gameObject.SetActive(false);
            stunOverlay.enabled = false;
        }
    }

    public override void ReduceReviveTime(float reduceTime = 1.0f)
    {
        if (photonView.IsMine)
        {
            OnHitEvent.Invoke();
            stunElapsed += reduceTime;
        }
        else
        {
            photonView.RPC("RPC_ReduceReviveTime", photonView.Owner, reduceTime);
        }
    }

    [PunRPC]
    public void RPC_ReduceReviveTime(float reduceTime)
    {
        ReduceReviveTime(reduceTime);
    }

    public override bool IsStunState()
    {
        return currentState == PinkPlayerState.Stun;
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

            SetTriggerParameter("revive");

            currentState = PinkPlayerState.Idle;

            if (photonView.IsMine)
            {
                stunSlider.gameObject.SetActive(false);
                stunOverlay.enabled = false;
                hpBar.enabled = true;
            }

            animator.SetBool("revive", true);
            //if (PhotonNetwork.IsConnected)
            //{
            //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "revive", true);
            //}

            photonView.RPC("UpdateHP", RpcTarget.All, 20f); // 여기서 체력 업데이트
            Debug.Log("플레이어 부활");

            if (PhotonNetworkManager.Instance != null)
            {
                PhotonNetworkManager.Instance.ReportPlayerRevive(photonView.Owner.ActorNumber);
            }
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

        if (PhotonNetworkManager.Instance != null)
        {
            PhotonNetworkManager.Instance.ReportPlayerDeath(photonView.Owner.ActorNumber);
        }

        if (photonView.IsMine)
        {
            stunSlider.gameObject.SetActive(false);
            stunOverlay.enabled = false;
            hpBar.enabled = false;  // 사망시 체력바 비활성화
        }

        if (animator != null)
        {
            animator.SetBool("die", true);
            //if (PhotonNetwork.IsConnected)
            //{
            //    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "die", true);
            //}
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
        playerBlessing.FindSkillEffect(runTimeData.skillWithLevel[(int)Skills.Mouse_R].skillData.ID, this).ApplyEffect();
    }
}
