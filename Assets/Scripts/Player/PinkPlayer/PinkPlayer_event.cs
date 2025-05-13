using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PinkPlayercontroller_event : Playercontroller_event
{
    private PinkPlayerController pinkPlayerController;

    public event Action<InputAction.CallbackContext> OnMoveEvent;
    public event Action<InputAction.CallbackContext> OnInteractionEvent;

    public UnityEvent OnMouseLEvent;
    public UnityEvent OnMouseREvent;
    public UnityEvent OnKeyboardShiftLEvent;
    public UnityEvent OnKeyboardREvent;

    private Action<InputAction.CallbackContext> movePerformed;
    private Action<InputAction.CallbackContext> moveCanceled;
    private Action<InputAction.CallbackContext> dashPerformed;
    private Action<InputAction.CallbackContext> dashCanceled;
    private Action<InputAction.CallbackContext> interactionPerformed;
    private Action<InputAction.CallbackContext> interactionCanceled;
    private Action<InputAction.CallbackContext> interactionStarted;
    private Action<InputAction.CallbackContext> basicAttackPerformed;
    private Action<InputAction.CallbackContext> specialAttackStarted;
    private Action<InputAction.CallbackContext> specialAttackCanceled;
    private Action<InputAction.CallbackContext> specialAttackPerformed;
    private Action<InputAction.CallbackContext> skillAttackPerformed;
    private Action<InputAction.CallbackContext> ultimateAttackStarted;

    private void Start()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            Debug.Log("내가 아님");
            return;
        }

        movePerformed = OnMove;
        moveCanceled = OnMove;
        dashPerformed = OnKeyboard_Spacebar;
        dashCanceled = OnKeyboard_Spacebar;
        interactionPerformed = OnInteraction;
        interactionCanceled = OnInteraction;
        interactionStarted = OnInteraction;
        basicAttackPerformed = OnMouse_L;
        specialAttackStarted = OnMouse_R;
        specialAttackCanceled = OnMouse_R;
        specialAttackPerformed = OnMouse_R;
        skillAttackPerformed = OnKeyboard_Shift_L;
        ultimateAttackStarted = OnKeyboard_R;

        var actions = InputManager.Instance.PlayerInput.actions;
        actions["Move"].performed += movePerformed;
        actions["Move"].canceled += moveCanceled;
        actions["Dash"].performed += dashPerformed;
        actions["Dash"].canceled += dashCanceled;
        actions["Interaction"].performed += interactionPerformed;
        actions["Interaction"].canceled += interactionCanceled;
        actions["Interaction"].started += interactionStarted;
        actions["BasicAttack"].performed += basicAttackPerformed;
        actions["SpecialAttack"].started += specialAttackStarted;
        actions["SpecialAttack"].canceled += specialAttackCanceled;
        actions["SpecialAttack"].performed += specialAttackPerformed;
        actions["SkillAttack"].performed += skillAttackPerformed;
        actions["UltimateAttack"].started += ultimateAttackStarted;

    }
    private void Awake()
    {
        pinkPlayerController = GetComponent<PinkPlayerController>();
        if (pinkPlayerController == null)
        {
            Debug.LogError("WhitePlayerController 컴포넌트가 없습니다!");
        }
    }

    // 이동 (WASD)
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 moveInput;
            if (!isInVillage)
            {
                moveInput = context.ReadValue<Vector2>();
            }
            else
            {
                moveInput = new Vector2(context.ReadValue<Vector2>().x, 0);
            }
            pinkPlayerController.SetMoveInput(moveInput);
            OnMoveEvent?.Invoke(context);
        }
        else if (context.canceled)
        {
            pinkPlayerController.SetMoveInput(Vector2.zero);
        }
    }

    // 상호작용 (F키)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        OnInteractionEvent?.Invoke(context);
    }

    // 마우스 왼쪽 클릭 (평타)
    public void OnMouse_L(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            pinkPlayerController.HandleNormalAttack();
            OnMouseLEvent?.Invoke();
        }
    }

    // 마우스 오른쪽 클릭 (핑크 플레이어 우클릭)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        if (isInVillage) return;


        // 1) performed 단계에서만 콤보 취소(tackle) 시도
        if (context.performed)
        {
            bool inBasic = pinkPlayerController.currentState == PinkPlayerState.BasicAttack;
            // CancleState가 2일 때만
            bool canCancel = pinkPlayerController.animator.GetBool("CancleState2");

            if (inBasic && canCancel)
            {
                // tackle 상태로 전환
                pinkPlayerController.HandleCharge();
                OnMouseREvent?.Invoke();
                return;
            }
            else if (!inBasic)
            {
                pinkPlayerController.StartCharge();
            }
        }

        // 2) 그 외에는 Hold 차지 공격
        if (context.started)
        {
        }
        else if (context.canceled)
        {
            pinkPlayerController.ReleaseCharge();
        }
    }


    // 좌 Shift 키 (특수 공격)
    public void OnKeyboard_Shift_L(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            pinkPlayerController.HandleSpecialAttack();
            OnKeyboardShiftLEvent?.Invoke();
        }
    }

    // R 키 (궁극기)
    public void OnKeyboard_R(InputAction.CallbackContext context)
    {
        pinkPlayerController.OnUltimateInput(context);
        OnKeyboardREvent?.Invoke();

    }

    // Space바 (회피)
    public void OnKeyboard_Spacebar(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            pinkPlayerController.HandleDash();
            OnKeyboardREvent?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (!photonView.IsMine)
            return;

        if (InputManager.Instance != null)
        {
            var actions = InputManager.Instance.PlayerInput.actions;
            actions["Move"].performed -= movePerformed;
            actions["Move"].canceled -= moveCanceled;
            actions["Dash"].performed -= dashPerformed;
            actions["Dash"].canceled -= dashCanceled;
            actions["Interaction"].performed -= interactionPerformed;
            actions["Interaction"].canceled -= interactionCanceled;
            actions["Interaction"].started -= interactionStarted;
            actions["BasicAttack"].performed -= basicAttackPerformed;
            actions["SpecialAttack"].started -= specialAttackStarted;
            actions["SpecialAttack"].canceled -= specialAttackCanceled;
            actions["SpecialAttack"].performed -= specialAttackPerformed;
            actions["SkillAttack"].performed -= skillAttackPerformed;
            actions["UltimateAttack"].started -= ultimateAttackStarted;
        }
    }
}
