using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WhitePlayercontroller_event : MonoBehaviour
{
    private WhitePlayerController whitePlayerController;

    public UnityEvent OnMoveEvent;
    public UnityEvent OnInteractionEvent;
    public UnityEvent OnMouseLEvent;
    public UnityEvent OnMouseREvent;
    public UnityEvent OnKeyboardShiftLEvent;
    public UnityEvent OnKeyboardREvent;


    private void Start()
    {
        InputManager.Instance.PlayerInput.actions["Move"].performed += ctx => OnMove(ctx);
        InputManager.Instance.PlayerInput.actions["Move"].canceled += ctx => OnMove(ctx);
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
            Vector2 moveInput = context.ReadValue<Vector2>();
            whitePlayerController.SetMoveInput(moveInput);
            OnMoveEvent?.Invoke();
        }
        else if (context.canceled)
        {
            whitePlayerController.SetMoveInput(Vector2.zero);
        }
    }

    // ��ȣ�ۿ� (FŰ)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("��ȣ�ۿ� ȣ���.");
            OnInteractionEvent?.Invoke();
        }
    }

    // ���콺 ���� Ŭ�� (��Ÿ)
    public void OnMouse_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleNormalAttack();
            OnMouseLEvent?.Invoke();
        }
    }

    // ���콺 ������ Ŭ�� (����)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleGuard();
            OnMouseREvent?.Invoke();
        }
    }

    // �� Shift Ű (Ư�� ����)
    public void OnKeyboard_Shift_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleSpecialAttack();
            OnKeyboardShiftLEvent?.Invoke();
        }
    }

    // R Ű (�ñر�)
    public void OnKeyboard_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleUltimateAttack();
            OnKeyboardREvent?.Invoke();
        }
    }
}
