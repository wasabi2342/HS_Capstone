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
            // 기절 상태라면 부활 로직 실행
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // 체력을 20으로 회복
                whitePlayer.currentHealth = 20;
                // Revive() 메서드 호출
                whitePlayer.Revive();
                Debug.Log("플레이어 부활 상호작용");
            }
        }
    }
}

