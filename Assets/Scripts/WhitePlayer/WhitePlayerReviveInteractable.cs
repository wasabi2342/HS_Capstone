using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer;

    private void Awake()
    {
        whitePlayer = GetComponent<WhitePlayerController>();    
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 기절이면 부활 로직 실행
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // 체력 20으로 회복
                whitePlayer.currentHealth = 20;
                // Revive() 메서드 호출
                whitePlayer.Revive();
                Debug.Log("플레이어 부활 상호작용");
            }
        }
    }
}
