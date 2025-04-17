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

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        base.OnInteract(ctx);
        //whitePlayer.HandleReviveInteraction(ctx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(whitePlayer.currentState != WhitePlayerState.Stun)
        {
            return;
        }

        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    protected override void OnPerformedEvent()
    {
        base.OnPerformedEvent();
        whitePlayer.Revive();
    }

    protected override void OnCanceledEvent()
    {
        base.OnCanceledEvent();
    }

    protected override void OnStartedEvent()
    {
        base.OnStartedEvent();
    }
}
