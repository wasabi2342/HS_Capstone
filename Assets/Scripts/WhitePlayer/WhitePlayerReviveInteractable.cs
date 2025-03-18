// WhitePlayerReviveInteractable.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer;

    private void Awake()
    {
        // WhitePlayerController�� ������
        whitePlayer = GetComponent<WhitePlayerController>();
        if (whitePlayer == null)
        {
            Debug.LogError("WhitePlayerController ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // ���� ����(Stun)��� ��Ȱ ���� ����
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // ü�� 20���� ȸ��
                //whitePlayer.currentHealth = 20;
                // Revive() �޼��� ȣ��
                whitePlayer.Revive();
                Debug.Log("��Ȱ ��ȣ�ۿ� �����.");
            }
        }
    }
}
