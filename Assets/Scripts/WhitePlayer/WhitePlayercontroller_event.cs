using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WhitePlayercontroller_event : MonoBehaviour
{
    private PlayerController playerController;

    
    public UnityEvent OnMoveEvent;
    public UnityEvent OnInteractionEvent;
    public UnityEvent OnMouseLEvent;
    public UnityEvent OnMouseREvent;
    public UnityEvent OnKeyboardShiftLEvent;
    public UnityEvent OnKeyboardREvent;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController 컴포넌트가 없습니다!");
        }
    }

    // 이동 인풋 ( WASD)
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.OnMove(context);
            OnMoveEvent?.Invoke();
        }
    }

    // 상호작용 인풋 (F키)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.OnNPCInteract(context);
            OnInteractionEvent?.Invoke();
        }
    }

    // 마우스 왼쪽 클릭 (평타)
    public void OnMouse_L()
    {
        playerController.HandleNormalAttack();
        OnMouseLEvent?.Invoke();
    }

    // 마우스 오른쪽 클릭 (가드, 데미지가 안 들어올 때 실행)
    public void OnMouse_R()
    {
        playerController.HandleGuard();
        OnMouseREvent?.Invoke();
    }

    // 좌 Shift 키 (특수 공격: 평타보다 강한 공격)
    public void OnKeyboard_Shift_L()
    {
        playerController.HandleSpecialAttack();
        OnKeyboardShiftLEvent?.Invoke();
    }

    // R 키 (궁극기: 평타, 특수공격보다 더 강한 공격)
    public void OnKeyboard_R()
    {
        playerController.HandleUltimateAttack();
        OnKeyboardREvent?.Invoke();
    }
}
