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
            Debug.Log("���� �ƴ�");
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
            Debug.LogError("WhitePlayerController ������Ʈ�� �����ϴ�!");
        }
    }

    // �̵� (WASD)
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

    // ��ȣ�ۿ� (FŰ)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        OnInteractionEvent?.Invoke(context);
    }

    // ���콺 ���� Ŭ�� (��Ÿ)
    public void OnMouse_L(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleNormalAttack();
            OnMouseLEvent?.Invoke();

            // katana_slash ���� ���
            //AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_slash", transform.position);

        }
    }

    // ���콺 ������ Ŭ�� (����)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleGuard();
            OnMouseREvent?.Invoke();
        }
    }

    // �� Shift Ű (Ư�� ����)
    public void OnKeyboard_Shift_L(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleSpecialAttack();
            OnKeyboardShiftLEvent?.Invoke();

            // katana_stab ���
            //AudioManager.Instance.PlayOneShot("event:/Character/Character-sword/katana_stab", transform.position);
        }
    }

    // R Ű (�ñر�)
    public void OnKeyboard_R(InputAction.CallbackContext context)
    {
        if (context.performed && !isInVillage)
        {
            whitePlayerController.HandleUltimateAttack();
            OnKeyboardREvent?.Invoke();
        }
    }

    // Space�� (ȸ��)
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
