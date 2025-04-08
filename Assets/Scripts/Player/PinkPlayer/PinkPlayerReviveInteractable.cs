using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinkPlayerReviveInteractable : GaugeInteraction
{
    private PinkPlayerController pinkPlayer;

    private void Awake()
    {
        pinkPlayer = GetComponentInParent<PinkPlayerController>();
    }

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        base.OnInteract(ctx);
        //whitePlayer.HandleReviveInteraction(ctx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (pinkPlayer.currentState != PinkPlayerState.Stun)
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
        pinkPlayer.Revive();
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
