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
            Debug.LogError("WhitePlayerController 컴포넌트가 없습니다!");
        }
    }

    // 이동 (WASD)
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

    // 상호작용 (F키)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("상호작용 호출됨.");
            OnInteractionEvent?.Invoke();
        }
    }

    // 마우스 왼쪽 클릭 (평타)
    public void OnMouse_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleNormalAttack();
            OnMouseLEvent?.Invoke();
        }
    }

    // 마우스 오른쪽 클릭 (가드)
    public void OnMouse_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleGuard();
            OnMouseREvent?.Invoke();
        }
    }

    // 좌 Shift 키 (특수 공격)
    public void OnKeyboard_Shift_L(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleSpecialAttack();
            OnKeyboardShiftLEvent?.Invoke();
        }
    }

    // R 키 (궁극기)
    public void OnKeyboard_R(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            whitePlayerController.HandleUltimateAttack();
            OnKeyboardREvent?.Invoke();
        }
    }
}
