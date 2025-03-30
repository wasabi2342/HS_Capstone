using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum WhitePlayerState { Idle, Run, BasicAttack, Hit, Dash, Skill, Ultimate, Guard, Parry, Counter, Stun, Revive, Death }

public class WhitePlayerController : ParentPlayerController
{
    [Header("�뽬 ����")]
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;

    [Header("�߽��� ����")]
    [Tooltip("�⺻ CenterPoint (�ִϸ��̼� �̺�Ʈ ��� ���)")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;
    [Tooltip("8���� CenterPoint �迭 (����: 0=��, 1=���, 2=������, 3=����, 4=�Ʒ�, 5=����, 6=����, 7=�»�)")]
    public Transform[] centerPoints = new Transform[8];
    private int currentDirectionIndex = 0;

    private Image stunOverlay;
    private Image stunSlider;
    private Image hpBar;

    [Header("��Ȱ UI ����")]
    public Canvas reviveCanvas;
    public Image reviveGauge;
    private Coroutine reviveCoroutine;
    private bool isInReviveRange = false;
    private WhitePlayerController stunnedPlayer;

    public WhitePlayerState currentState = WhitePlayerState.Idle;
    public WhitePlayerState nextState = WhitePlayerState.Idle;

    [Header("��Ŭ�� ����/�и� ����")]
    public float guardDuration = 2f;
    public float parryDuration = 2f;

    [Header("Counter (�ߵ�) ����")]
    public int counterDamage = 20;

    private AttackManager attackManager;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Vector2 moveInput;

    public WhitePlayerAttackZone AttackCollider;

    private GaugeInteraction gaugeInteraction;

    private Coroutine stunCoroutine;

    private float maxHealth = 100f;

    protected override void Awake()
    {
        AttackCollider = GetComponentInChildren<WhitePlayerAttackZone>();
        base.Awake();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�! (WhitePlayerController)");
        }

        attackManager = gameObject.AddComponent<AttackManager>();
    }

    private void Start()
    {
        currentState = WhitePlayerState.Idle;

        if (photonView.IsMine)
        {
            // UI 요소를 찾을 때 null 체크 추가
            GameObject stunOverlayObj = GameObject.Find("StunOverlay");
            if (stunOverlayObj != null)
                stunOverlay = stunOverlayObj.GetComponent<Image>();
            
            GameObject stunSliderObj = GameObject.Find("StunTimeBar");
            if (stunSliderObj != null)
                stunSlider = stunSliderObj.GetComponent<Image>();
            
            GameObject hpBarObj = GameObject.Find("HPImage");
            if (hpBarObj != null)
                hpBar = hpBarObj.GetComponent<Image>();

            // UI 컴포넌트가 존재하는 경우에만 활성화 상태 변경
            if (stunOverlay != null)
                stunOverlay.enabled = false;
            
            if (stunSlider != null)
                stunSlider.enabled = false;
            
            if (hpBar != null)
                hpBar.enabled = true;

            // GaugeInteraction 컴포넌트를 찾을 때도 null 체크 필요 없음 (GetComponentInChildren는 null을 반환할 수 있음)
            gaugeInteraction = GetComponentInChildren<GaugeInteraction>();

            var eventController = GetComponent<WhitePlayercontroller_event>();
            if (eventController != null)
            {
                //eventController.OnInteractionEvent += HandleReviveInteraction;
            }
        }

        // attackManager가 null이 아닌지 확인
        if (attackManager != null)
            attackManager.Initialize(this);
        else
            Debug.LogError("AttackManager is null! Make sure it's properly added in Awake().");
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

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

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

    public void HandleNormalAttack()
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
            currentState = WhitePlayerState.Counter;

            ResetSkillCooldown(Skills.Mouse_R);
            return;
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

        if (attackManager.ExecuteAction(Skills.Mouse_L))
        {
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

    public void HandleDash()
    {
        attackManager.ExecuteAction(Skills.Space);
    }

    public void HandleSpecialAttack()
    {
        attackManager.ExecuteAction(Skills.Shift_L);
    }

    public void HandleUltimateAttack()
    {
        attackManager.ExecuteAction(Skills.R);
    }

    public void HandleGuard()
    {
        attackManager.ExecuteAction(Skills.Mouse_R);
    }

    public void AdvanceAttackCombo()
    {
        attackStack++;
        AttackStackUpdate?.Invoke(attackStack);
        attackManager.AdvanceCombo(attackStack);
    }

    public Vector3 GetMouseWorldPosition()
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

    public IEnumerator DoDash(Vector3 dashDir)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + dashDir.normalized * dashDistance;
        yield return null;
        transform.position = targetPos;
    }

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

                currentState = WhitePlayerState.Parry;
                ResetSkillCooldown(Skills.Mouse_R);
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
                animator.SetBool("hit", true);
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("SyncBoolParameter", RpcTarget.Others, "hit", true);
                }
                currentState = WhitePlayerState.Hit;
            }
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

        runTimeData.currentHealth = hp;

        if (photonView.IsMine && hpBar != null)
        {
            hpBar.enabled = true;
            hpBar.fillAmount = runTimeData.currentHealth / maxHealth;
        }

        Debug.Log(photonView.ViewID + " 플레이어 체력 업데이트됨: " + runTimeData.currentHealth);
    }

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
            if (stunOverlay != null)
                stunOverlay.enabled = true;
            
            if (stunSlider != null)
            {
                stunSlider.enabled = true;
                stunSlider.fillAmount = 1f;
            }
            
            if (hpBar != null)
                hpBar.enabled = false;
        }

        while (elapsed < stunDuration && currentState == WhitePlayerState.Stun)
        {
            elapsed += Time.deltaTime;

            if (photonView.IsMine && stunSlider != null)
            {
                stunSlider.fillAmount = 1 - (elapsed / stunDuration);
            }

            yield return null;
        }

        if (currentState == WhitePlayerState.Stun)
        {
            TransitionToDeath();
        }

        if (photonView.IsMine)
        {
            if (stunSlider != null)
                stunSlider.enabled = false;
            
            if (stunOverlay != null)
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
                if (stunSlider != null)
                    stunSlider.enabled = false;
                
                if (stunOverlay != null)
                    stunOverlay.enabled = false;
                
                if (hpBar != null)
                    hpBar.enabled = true;
            }

            animator.SetBool("revive", true);
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("SyncBoolParameter", RpcTarget.Others, "revive", true);
            }

            photonView.RPC("UpdateHP", RpcTarget.All, 20f);
            Debug.Log("플레이어 부활");
        }
    }

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
            if (stunSlider != null)
                stunSlider.enabled = false;
            
            if (stunOverlay != null)
                stunOverlay.enabled = false;
            
            if (hpBar != null)
                hpBar.enabled = false;
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

    // 쿨다운 시작 메서드 - AttackAction에서 호출
    public void StartCooldown(Skills skillType)
    {
        switch(skillType)
        {
            case Skills.Mouse_L:
                StartAttackCooldown();
                break;
            case Skills.Mouse_R:
                StartMouseRightCooldown();
                break;
            case Skills.Space:
                StartDashCooldown();
                break;
            case Skills.Shift_L:
                StartShiftCooldown();
                break;
            case Skills.R:
                StartUltimateCooldown();
                break;
        }
    }
    
    #region Animation Event Methods
    
    // 기본 공격 애니메이션 이벤트 메서드
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
    
    public void OnAttack1DamageEnd()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        Debug.Log("Attack1: 데미지 종료");
    }
    
    public void OnAttack1AllowNextInput()
    {
        OnAttackAllowNextInput();
    }
    
    public void OnAttack1AnimationEnd()
    {
        OnAttackAnimationEnd();
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
    
    public void OnAttack2DamageEnd()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        Debug.Log("Attack2: 데미지 종료");
    }
    
    public void OnAttack2AllowNextInput()
    {
        OnAttackAllowNextInput();
    }
    
    public void OnAttack2AnimationEnd()
    {
        OnAttackAnimationEnd();
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
    
    // 스킬 관련 메서드
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
    
    // 궁극기 이펙트 메서드
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
    
    // 공격 애니메이션에서 다음 입력을 허용하는 상태로 전환하는 메서드
    public void OnAttackAllowNextInput()
    {
        animator.SetBool("FreeState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "FreeState", true);
        }
        Debug.Log("다음 입력 허용");
    }
    
    // 공격 애니메이션 상태 변경 메서드
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
        Debug.Log("애니메이션 종료");
    }
    
    // 공격 스택을 초기화하는 메서드 (애니메이션 이벤트에서 호출)
    public void InitAttackStak()
    {
        attackStack = 0;
        AttackStackUpdate?.Invoke(attackStack);
        Debug.Log("공격 스택 초기화");
    }
    
    // 공격 준비 상태 시작 메서드 (오타가 있지만 애니메이션 이벤트와 이름 일치를 위해 유지)
    public void OnAttackPreAttckStart()
    {
        animator.SetBool("CancleState", true);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", true);
        }
        Debug.Log("공격 준비 시작");
    }
    
    // 공격 준비 상태 종료 메서드
    public void OnAttackPreAttckEnd()
    {
        animator.SetBool("CancleState", false);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("SyncBoolParameter", RpcTarget.Others, "CancleState", false);
        }
        Debug.Log("공격 준비 종료");
    }
    
    // 공격 시 캐릭터를 특정 방향으로 이동시키는 메서드
    public void OnMoveFront(float value)
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 direction = (mousePos - transform.position).normalized;
        transform.Translate(direction * value, Space.World);
        Debug.Log($"전방 이동: {value}");
    }
    
    // 마지막 공격 시작 처리
    public void OnLastAttackStart()
    {
        Debug.Log("마지막 공격 시작");
        // 마지막 공격 효과 또는 로직 추가
    }
    
    // 공격4 콜라이더 제거
    public void OnCollider4Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        Debug.Log("공격 4 콜라이더 비활성화");
    }
    
    // 공격3 콜라이더 제거
    public void OnCollider3Delete()
    {
        if (AttackCollider != null)
        {
            AttackCollider.EnableAttackCollider(false);
        }
        Debug.Log("공격 3 콜라이더 비활성화");
    }
    
    // 스킬 쿨다운 시작
    public void CoStartSkillCoolDown()
    {
        StartShiftCooldown();
        Debug.Log("스킬 쿨다운 시작");
    }
    
    // 궁극기 쿨다운 시작
    public void CoStartUltimateCoolDown()
    {
        StartUltimateCooldown();
        Debug.Log("궁극기 쿨다운 시작");
    }
    
    // 궁극기 이동 처리
    public void UltimateMove(float distance)
    {
        Vector3 direction = animator.GetBool("Right") ? Vector3.right : Vector3.left;
        transform.Translate(direction * distance, Space.World);
        Debug.Log($"궁극기 이동: {distance}");
    }
    
    #endregion
}
