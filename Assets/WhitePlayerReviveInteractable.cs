using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : GaugeInteraction
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
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                
                whitePlayer.photonView.RPC("ReviveRPC", RpcTarget.MasterClient);
                Debug.Log("플레이어 부활 RPC 호출됨");
            }
        }
    }
}
