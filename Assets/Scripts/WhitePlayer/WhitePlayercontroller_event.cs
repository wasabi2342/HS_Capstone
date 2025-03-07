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
            Debug.LogError("PlayerController ������Ʈ�� �����ϴ�!");
        }
    }

    // �̵� ��ǲ ( WASD)
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.OnMove(context);
            OnMoveEvent?.Invoke();
        }
    }

    // ��ȣ�ۿ� ��ǲ (FŰ)
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerController.OnNPCInteract(context);
            OnInteractionEvent?.Invoke();
        }
    }

    // ���콺 ���� Ŭ�� (��Ÿ)
    public void OnMouse_L()
    {
        playerController.HandleNormalAttack();
        OnMouseLEvent?.Invoke();
    }

    // ���콺 ������ Ŭ�� (����, �������� �� ���� �� ����)
    public void OnMouse_R()
    {
        playerController.HandleGuard();
        OnMouseREvent?.Invoke();
    }

    // �� Shift Ű (Ư�� ����: ��Ÿ���� ���� ����)
    public void OnKeyboard_Shift_L()
    {
        playerController.HandleSpecialAttack();
        OnKeyboardShiftLEvent?.Invoke();
    }

    // R Ű (�ñر�: ��Ÿ, Ư�����ݺ��� �� ���� ����)
    public void OnKeyboard_R()
    {
        playerController.HandleUltimateAttack();
        OnKeyboardREvent?.Invoke();
    }
}
