using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WhitePlayercontroller_event : Playercontroller_event
{
    private WhitePlayerController whitePlayerController;

    public event Action<InputAction.CallbackContext> OnMoveEvent;
    public event Action<InputAction.CallbackContext> OnInteractionEvent;

    public UnityEvent OnMouseLEvent;
    public UnityEvent OnMouseREvent;
    public UnityEvent OnKeyboardShiftLEvent;
    public UnityEvent OnKeyboardREvent;

    private Action<InputAction.CallbackContext> movePerformedCallback;
    private Action<InputAction.CallbackContext> moveCanceledCallback;
    private Action<InputAction.CallbackContext> dashPerformedCallback;
    private Action<InputAction.CallbackContext> dashCanceledCallback;
    private Action<InputAction.CallbackContext> interactionPerformedCallback;
    private Action<InputAction.CallbackContext> interactionCanceledCallback;
    private Action<InputAction.CallbackContext> interactionStartedCallback;
    private Action<InputAction.CallbackContext> basicAttackPerformedCallback;
    private Action<InputAction.CallbackContext> specialAttackPerformedCallback;
    private Action<InputAction.CallbackContext> skillAttackPerformedCallback;
    private Action<InputAction.CallbackContext> ultimateAttackPerformedCallback;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            Debug.Log("내가 아님");
            return;
        }

        movePerformedCallback = OnMove;
        moveCanceledCallback = OnMove;
        dashPerformedCallback = OnKeyboard_Spacebar;
        dashCanceledCallback = OnKeyboard_Spacebar;
        interactionPerformedCallback = OnInteraction;
        interactionCanceledCallback = OnInteraction;
        interactionStartedCallback = OnInteraction;
        basicAttackPerformedCallback = OnMouse_L;
        specialAttackPerformedCallback = OnMouse_R;
        skillAttackPerformedCallback = OnKeyboard_Shift_L;
        ultimateAttackPerformedCallback = OnKeyboard_R;

        InputManager.Instance.PlayerInput.actions["Move"].performed += movePerformedCallback;
        InputManager.Instance.PlayerInput.actions["Move"].canceled += moveCanceledCallback;
        //InputManager.Instance.PlayerInput.actions["Dash"].performed += dashPerformedCallback;
        //InputManager.Instance.PlayerInput.actions["Dash"].canceled += dashCanceledCallback;
        InputManager.Instance.PlayerInput.actions["Dash"].started += dashPerformedCallback;
        InputManager.Instance.PlayerInput.actions["Interaction"].performed += interactionPerformedCallback;
        InputManager.Instance.PlayerInput.actions["Interaction"].canceled += interactionCanceledCallback;
        InputManager.Instance.PlayerInput.actions["Interaction"].started += interactionStartedCallback;
        InputManager.Instance.PlayerInput.actions["BasicAttack"].performed += basicAttackPerformedCallback;
        InputManager.Instance.PlayerInput.actions["SpecialAttack"].performed += specialAttackPerformedCallback;
        InputManager.Instance.PlayerInput.actions["SkillAttack"].performed += skillAttackPerformedCallback;
        InputManager.Instance.PlayerInput.actions["UltimateAttack"].performed += ultimateAttackPerformedCallback;

    }
    private void Awake()
    {
        whitePlayerController = GetComponent<WhitePlayerController>();
        if (whitePlayerController == null)
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
            whitePlayerController.SetMoveInput(moveInput);
            OnMoveEvent?.Invoke(context);
        }
        else if (context.canceled)
        {
            whitePlayerController.SetMoveInput(Vector2.zero);
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
            whitePlayerController.HandleNormalAttack();
            OnMouseLEvent?.Invoke();

            // katana_slash 사운드 재생
            //AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_slash", transform.position);

        }
    }

    // 마우스 오른쪽 클릭 (가드)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleGuard();
            OnMouseREvent?.Invoke();
        }
    }

    // 좌 Shift 키 (특수 공격)
    public void OnKeyboard_Shift_L(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleSpecialAttack();
            OnKeyboardShiftLEvent?.Invoke();

            // katana_stab 재생
            //AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_stab", transform.position);
        }
    }

    // R 키 (궁극기)
    public void OnKeyboard_R(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleUltimateAttack();
            OnKeyboardREvent?.Invoke();
        }
    }

    // Space바 (회피)
    public void OnKeyboard_Spacebar(InputAction.CallbackContext context)
    {
        if (context.started && !isInVillage)
        {
            whitePlayerController.HandleDash();
            OnKeyboardREvent?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (!photonView.IsMine)
            return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerInput.actions["Move"].performed -= movePerformedCallback;
            InputManager.Instance.PlayerInput.actions["Move"].canceled -= moveCanceledCallback;
            //InputManager.Instance.PlayerInput.actions["Dash"].performed -= dashPerformedCallback;
            //InputManager.Instance.PlayerInput.actions["Dash"].canceled -= dashCanceledCallback;
            InputManager.Instance.PlayerInput.actions["Dash"].started -= dashPerformedCallback;
            InputManager.Instance.PlayerInput.actions["Interaction"].performed -= interactionPerformedCallback;
            InputManager.Instance.PlayerInput.actions["Interaction"].canceled -= interactionCanceledCallback;
            InputManager.Instance.PlayerInput.actions["Interaction"].started -= interactionStartedCallback;
            InputManager.Instance.PlayerInput.actions["BasicAttack"].performed -= basicAttackPerformedCallback;
            InputManager.Instance.PlayerInput.actions["SpecialAttack"].performed -= specialAttackPerformedCallback;
            InputManager.Instance.PlayerInput.actions["SkillAttack"].performed -= skillAttackPerformedCallback;
            InputManager.Instance.PlayerInput.actions["UltimateAttack"].performed -= ultimateAttackPerformedCallback;
        }
    }
}
