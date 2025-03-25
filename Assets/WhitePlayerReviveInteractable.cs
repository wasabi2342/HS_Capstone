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
        
        whitePlayer.HandleReviveInteraction(ctx);
    }
}
