using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // ���� ���¶�� ��Ȱ ���� ����
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // Revive() �޼��� ȣ��
                whitePlayer.Revive();
                Debug.Log("�÷��̾� ��Ȱ ��ȣ�ۿ�");
            }
        }
    }
}

