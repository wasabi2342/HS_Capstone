using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BasePlayerController : MonoBehaviourPunCallbacks
{
    #region RoomMovement에서 추가 부분

    [SerializeField]
    protected float portalCooldown;
    [SerializeField]
    protected CharacterStats characterData;

    protected bool canUsePortal;
    protected Vector3 portalExitPosition;

    protected InteractableObject nowObject;

    protected bool canControl;
    protected bool isInVillage;

    public Action startFillGauge;
    public Action<bool> canelFillGauge;

    public GameObject changeCharacterPrefab;
    public Dictionary<InputKey, (Blessing blessing, int level)> blessings;

    public Action<UIIcon, float> updateUIAction;
    public Action<UIIcon, Color> updateUIOutlineAction;

    #endregion

    [Header("기본 이동 속도")]
    public float speedHorizontal = 5f;
    public float speedVertical = 5f;

    [Header("대쉬 설정")]
    public float dashDuration = 0.2f;
    public float dashDistance = 2f;
    public float dashDoubleClickThreshold = 0.3f;
    protected float lastDashClickTime = -Mathf.Infinity;

    [Header("중심점 설정")]
    [Tooltip("기본 CenterPoint. Invoke 이벤트 등에서 사용.")]
    public Transform centerPoint;
    public float centerPointOffsetDistance = 0.5f;

    [Header("8방향 CenterPoints 설정")]
    [Tooltip("플레이어의 8방향 CenterPoint들을 할당 (순서: 0=위, 1=우상, 2=오른쪽, 3=우하, 4=아래, 5=좌하, 6=왼쪽, 7=좌상)")]
    public Transform[] centerPoints = new Transform[8];
    public int currentDirectionIndex = 0;

    [Header("상호작용 / 데미지 범위 설정")]
    public LayerMask interactionLayerMask;
    public float interactionRadius = 1.5f;

    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    protected int currentHealth = 0;

    // 함정 해제 관련
    protected int trapClearCount = 0;
    protected GameObject currentTrap = null;
    protected bool isTrapCleared = false;

    // 공격/데미지 관련
    protected bool isAttacking = false;
    protected bool isDead = false;

    // 이동 입력
    protected Vector2 moveInput;

    // PlayerState 열거형에 Guard와 Parry 상태 추가 (추가: Guard, Parry)
    public enum PlayerState { Idle, Run, Attack_L, Attack_R, Skill, Ultimate, Hit, Guard, Parry, Death }
    protected PlayerState currentState = PlayerState.Idle;

    protected Animator animator;
    protected bool isDashing = false;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(this);
    }

    protected virtual void Start()
    {
        //if (photonView != null && !photonView.IsMine)
        //{
        //    this.enabled = false;
        //}
        currentHealth = maxHealth;
        currentState = PlayerState.Idle;
        canControl = true;
        canUsePortal = true;
        isInVillage = true;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.InRoom)
        {
            Debug.Log("업데이트에서 리턴 실행됨");
            return;

        }
        if (isDead || currentState == PlayerState.Death) return;

        // 8방향 CenterPoint 갱신
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

        CheckDashInput();
        HandleMovement();
        HandleActions();
    }

    public virtual void OnMove(InputAction.CallbackContext context)
    {
        if (photonView.IsMine || !PhotonNetwork.InRoom)
            moveInput = context.ReadValue<Vector2>();
    }

    protected virtual void CheckDashInput()
    {
        if (isInVillage)
            return;
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

    protected virtual void HandleMovement()
    {
        if (currentState == PlayerState.Death) return;

        float h = moveInput.x;
        float v = moveInput.y;
        bool isMoving = (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
        currentState = isMoving ? PlayerState.Run : PlayerState.Idle;

        if (isMoving)
        {
            Vector3 moveDir;
            if (isInVillage)
                moveDir = new Vector3(h, 0, 0).normalized;
            else
                moveDir = new Vector3(h, 0, v).normalized;

            transform.Translate(moveDir * speedVertical * Time.deltaTime, Space.World);
            //transform.Translate(moveDir * speedVertical * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
            animator.SetFloat("moveX", h);
            animator.SetFloat("moveY", v);
        }
    }

    protected virtual IEnumerator DoDash(Vector3 dashDir)
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
        yield return null;
    }

    protected virtual void HandleActions()
    {
        // 자식에서 구현
    }

    public virtual void OnNPCInteract(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine && PhotonNetwork.InRoom) return;
        //if (!context.performed) return;

        if (centerPoint == null) return;
        Vector3 checkPos = centerPoint.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                Debug.Log($"[BasePlayerController] NPC와 상호작용! : {col.name}");
            }
        }
        if (canControl && (!PhotonNetwork.InRoom || photonView.IsMine))
        {
            Debug.Log("OnInteract 호출됨!");
            switch (nowObject)
            {
                case InteractableObject.portal:
                    if (canUsePortal)
                    {
                        Debug.Log("포탈 사용");
                        UsePortal();
                    }
                    break;
                case InteractableObject.gameStart:
                    if (!RoomManager.Instance.isEnteringStage)
                    {
                        if (!RoomManager.Instance.IsPlayerInRestrictedArea())
                        {
                            RoomManager.Instance.InteractWithDungeonNPC().onClose += () => canControl = true;
                            canControl = false;
                        }
                        else
                        {
                            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("모든 플레이어가 밖으로 나와야 합니다.");
                        }
                    }
                    break;
                case InteractableObject.upgrade: // 업그래이드 UI생성하기
                    break;
                case InteractableObject.selectCharacter: // 캐릭터 선택 UI생성하기
                    RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                    canControl = false;

                    UIManager.Instance.OpenPopupPanel<UISelectCharacterPanel>().onClose += () =>
                    {
                        RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                        canControl = true;
                    };
                    break;
                case InteractableObject.changeSkill: // 스킬변경 UI 생성하기
                    break;
                case InteractableObject.trainingRoom:
                    if (isInVillage) // 훈련소로 화면 전환
                    {
                        RoomManager.Instance.EnterRestrictedArea(GetComponent<PhotonView>().ViewID);
                        EnterTrainingRoom();
                        isInVillage = false;
                    }
                    else // 훈련소에서 나오기
                    {
                        RoomManager.Instance.ExitRestrictedArea(GetComponent<PhotonView>().ViewID);
                        ExitTrainingRoom();
                        isInVillage = true;
                    }
                    break;
                case InteractableObject.mannequin:
                    if (context.started)
                    {
                        startFillGauge?.Invoke();
                    }
                    else if (context.canceled)
                    {
                        canelFillGauge?.Invoke(false);
                    }
                    else if (context.performed)
                    {
                        canelFillGauge?.Invoke(true);
                        //RoomManager.Instance.CreateCharacter(changeCharacterPrefab, transform.position, transform.rotation);
                        Destroy(gameObject);
                    }
                    break;
            }
        }
    }

    public virtual void OnTrapClear(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        if (!context.performed) return;

        if (centerPoint == null) return;
        Vector3 checkPos = centerPoint.position;
        Collider[] cols = Physics.OverlapSphere(checkPos, interactionRadius, interactionLayerMask);
        foreach (Collider col in cols)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Trap"))
            {
                trapClearCount++;
                Debug.Log($"[BasePlayerController] 함정 키 입력 횟수: {trapClearCount}");
                if (trapClearCount >= 2)
                {
                    isTrapCleared = true;
                    Debug.Log("[BasePlayerController] 함정 해제됨.");
                    Destroy(col.gameObject);
                    trapClearCount = 0;
                }
            }
        }
    }

    [PunRPC]
    protected void UpdateHP(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        updateUIAction?.Invoke(UIIcon.hpBar, (float)currentHealth / maxHealth);
       
        Debug.Log($"[BasePlayerController] 플레이어 체력: {currentHealth}");
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public virtual void TakeDamage(int damage)
    {
        /*
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        updateUIAction?.Invoke(UIIcon.hpBar, (float)currentHealth / maxHealth);
        */
        photonView.RPC("UpdateHP", RpcTarget.All, damage);

        //Debug.Log($"[BasePlayerController] 플레이어 체력: {currentHealth}");
        //if (currentHealth <= 0 && !isDead)
        //{
        //    Die();
        //}
    }

    public virtual void Die()
    {
        if (isDead) return;
        currentState = PlayerState.Death;
        isDead = true;
        Debug.Log("[BasePlayerController] 플레이어 사망!");

        isAttacking = false;
        isDashing = false;

        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
    }

    protected int DetermineDirectionIndex(Vector2 input)
    {
        if (input.magnitude < 0.01f) return currentDirectionIndex;
        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int idx = Mathf.RoundToInt(angle / 45f) % 8;
        return idx;
    }

    #region RoomMovement에서 가져온 함수
    private void UsePortal()
    {
        gameObject.transform.position = portalExitPosition;
        canUsePortal = false;
        StartCoroutine(PortalCooldownCheck(portalCooldown));
    }

    private IEnumerator PortalCooldownCheck(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canUsePortal = true;
    }

    private void EnterTrainingRoom()
    {
        canControl = false;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DORotate(new Vector3(0, -90, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.Append(transform.DOMoveZ(transform.position.z + 3, 1f));
        sequence.Append(transform.DOMoveX(20, 0.5f));
        //sequence.Append(transform.DORotate(new Vector3(90, 0, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.OnComplete(() => CanControl());
        sequence.Play();
    }

    private void ExitTrainingRoom()
    {
        canControl = false;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOMoveZ(transform.position.z - 3, 1f));
        sequence.Append(transform.DORotate(new Vector3(0, 90, 0), 1f, RotateMode.LocalAxisAdd));
        sequence.Append(transform.DOMove(new Vector3(transform.position.x, -0.35f, -0.35f), 0.5f));
        sequence.OnComplete(() => CanControl());
        sequence.Play();
    }

    private void CanControl()
    {
        canControl = true;
    }

    public void GetPortalExitPosition(Vector3 pos)
    {
        portalExitPosition = pos;
    }

    public void UpdateNowInteractable(InteractableObject obj)
    {
        nowObject = obj;
        Debug.Log(nowObject);
    }

    public string ReturnName()
    {
        return characterData.characterName;
    }

    public void InitBlessing()
    {
        blessings = new Dictionary<InputKey, (Blessing blessing, int level)>();

        for (int i = 0; i < 5; i++)
        {
            blessings.Add((InputKey)i, (Blessing.none, 0));
        }
    }
    #endregion
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "SampleScene")
        {
            isInVillage = false;
        }
        // 씬 로드 이후 초기화할 코드 추가 가능
    }
}
