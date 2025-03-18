// WhitePlayerReviveInteractable.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer;

    private void Awake()
    {
        // WhitePlayerController를 가져옴
        whitePlayer = GetComponent<WhitePlayerController>();
        if (whitePlayer == null)
        {
            Debug.LogError("WhitePlayerController 컴포넌트를 찾을 수 없습니다!");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 기절 상태(Stun)라면 부활 로직 실행
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // 체력 20으로 회복
                //whitePlayer.currentHealth = 20;
                // Revive() 메서드 호출
                whitePlayer.Revive();
                Debug.Log("부활 상호작용 실행됨.");
            }
        }
    }
}
