using UnityEngine;
using System.Collections;
using Photon.Pun;


public enum WhitePlayerState { Idle, Run, BasicAttack, Hit, Dash, Skill, Ultimate, Guard, Parry, Counter, Stun, Revive, Death }

public class WhitePlayerController : ParentPlayerController
{
    //  기본 이동 및 체력 관련 
    //[Header("이동 속도")]
    //public float speedHorizontal = 5f;
    //public float speedVertical = 5f;

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




    // 이동 입력 및 상태
    private Vector2 moveInput;
    public WhitePlayerState currentState = WhitePlayerState.Idle;
    public WhitePlayerState nextState = WhitePlayerState.Idle;

    [Header("우클릭 가드/패링 설정")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;
    //private bool isGuarding = false;
    //private bool isParrying = false;

    [Header("Counter (발도) 설정")]
    public int counterDamage = 20;

    // 참조 컴포넌트 
    private Animator animator;

    protected override void Awake()

    {
        AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();

        base.Awake();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다! (WhitePlayerController)");
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
        // 공격/스킬, 가드 등은 별도 스크립트에서 호출
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

    // 대쉬 처리

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
                Debug.Log("공격 스택 4 도달: 콤보 종료 및 초기화");
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

    // 특수 공격
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

    // 궁극기 공격 
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

    // 공격 애니메이션 이벤트용 스텁 (WhitePlayerController_AttackStack에서 호출) 

    public IEnumerator CoStartSkillCoolDown() // 이벤트 클립으로 쿨타임 체크
    {
        isShiftReady = false;
        ShiftCoolDownUpdate?.Invoke(shiftCoolDown);
        yield return new WaitForSeconds(shiftCoolDown);
        isShiftReady = true;
    }

    public IEnumerator CoStartUltimateCoolDown() // 이벤트 클립으로 쿨타임 체크
    {
        isUltimateReady = false;
        UltimateCoolDownUpdate?.Invoke(ultimateCoolDown);
        yield return new WaitForSeconds(ultimateCoolDown);
        isUltimateReady = true;
    }

    public IEnumerator CoStartGuardCoolDown() // 이벤트 클립으로 쿨타임 체크
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
        Debug.Log("Attack1: 데미지 시작");
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
        Debug.Log("후딜 시작");
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
        Debug.Log("Attack2: 데미지 시작");
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
        Debug.Log("Attack3: 데미지 시작");
    }

    public void OnCollider3Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);

            Debug.Log("Attack3: 첫번째 콜라이더 제거");
        }

        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 0.7f;
            }

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack3: 두번째 콜라이더 생성");
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
        Debug.Log("Attack4: 데미지 시작");
    }

    public void OnCollider4Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);

            Debug.Log("Attack4: 첫번째 콜라이더 제거");
        }

        if (AttackCollider != null)
        {
            if (photonView.IsMine)
            {
                AttackCollider.Damage = runTimeData.attackPower * 1.5f;
            }

            AttackCollider.EnableAttackCollider(true);
        }
        Debug.Log("Attack4: 두번째 콜라이더 생성");
    }


    public void InitAttackStak()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
    }

    // 가드/패링 처리
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

    // 피격 및 사망 처리
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
    }


    // 기절
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
        //photonView.RPC("PlayAnimation", RpcTarget.All, "stun");

        // 기절 상태 30초 동안의 코루틴 
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(CoStunDuration());
    }

    private IEnumerator CoStunDuration()
    {
        float stunDuration = 30f; // 빨리 보기위해 5초로 해둠, 나중에 30초로 조정하면 됩니다요.
        float elapsed = 0f;
        while (elapsed < stunDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        TransitionToDeath();
    }

    // 부활
    public void Revive()
    {
        // 만약 기절 상태라면 코루틴을 중지하고 상태를 Idle로 전환
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

            // 체력을 20으로 회복
            photonView.RPC("UpdateHP", RpcTarget.All, 20f);
            Debug.Log("플레이어 부활");
        }
    }

    // 사망 상태로 전환
    private void TransitionToDeath()
    {
        currentState = WhitePlayerState.Death;
        Debug.Log("플레이어 사망");
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
