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

    private void Start()
    {
        if (!photonView.IsMine)
            return;

        InputManager.Instance.PlayerInput.actions["Move"].performed += ctx => OnMove(ctx);
        InputManager.Instance.PlayerInput.actions["Move"].canceled += ctx => OnMove(ctx);
        InputManager.Instance.PlayerInput.actions["Dash"].performed += ctx => OnKeyboard_Spacebar(ctx);
        InputManager.Instance.PlayerInput.actions["Dash"].canceled += ctx => OnKeyboard_Spacebar(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].performed += ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].canceled += ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].started += ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["BasicAttack"].performed += ctx => OnMouse_L(ctx);
        InputManager.Instance.PlayerInput.actions["SpecialAttack"].performed += ctx => OnMouse_R(ctx);
        InputManager.Instance.PlayerInput.actions["SkillAttack"].performed += ctx => OnKeyboard_Shift_L(ctx);
        InputManager.Instance.PlayerInput.actions["UltimateAttack"].performed += ctx => OnKeyboard_R(ctx);

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

    // 마우스 오른쪽 클릭 (가드)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        //if (context.performed && !isInVillage)
        //{
        //    pinkPlayerController.HandleGuard();
        //    OnMouseREvent?.Invoke();
        //}
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
        if (context.performed && !isInVillage)
        {
            pinkPlayerController.HandleUltimateAttack();
            OnKeyboardREvent?.Invoke();
        }
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

        InputManager.Instance.PlayerInput.actions["Move"].performed -= ctx => OnMove(ctx);
        InputManager.Instance.PlayerInput.actions["Move"].canceled -= ctx => OnMove(ctx);
        InputManager.Instance.PlayerInput.actions["Dash"].performed -= ctx => OnKeyboard_Spacebar(ctx);
        InputManager.Instance.PlayerInput.actions["Dash"].canceled -= ctx => OnKeyboard_Spacebar(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].performed -= ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].canceled -= ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["Interaction"].started -= ctx => OnInteraction(ctx);
        InputManager.Instance.PlayerInput.actions["BasicAttack"].performed -= ctx => OnMouse_L(ctx);
        InputManager.Instance.PlayerInput.actions["SpecialAttack"].performed -= ctx => OnMouse_R(ctx);
        InputManager.Instance.PlayerInput.actions["SkillAttack"].performed -= ctx => OnKeyboard_Shift_L(ctx);
        InputManager.Instance.PlayerInput.actions["UltimateAttack"].performed -= ctx => OnKeyboard_R(ctx);
    }
}
