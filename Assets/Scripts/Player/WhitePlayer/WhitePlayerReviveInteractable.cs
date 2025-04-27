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

        Debug.Log("부활 상호작용 enter");

        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        Debug.Log("부활 상호작용 exit");
    }

    protected override void OnPerformedEvent()
    {
        base.OnPerformedEvent();
        
        Debug.Log("부활 상호작용 ");

        whitePlayer.Revive();
    }

    protected override void OnCanceledEvent()
    {
        base.OnCanceledEvent();
        
        Debug.Log("부활 상호작용 cancel");
    }

    protected override void OnStartedEvent()
    {
        base.OnStartedEvent();

        Debug.Log("부활 상호작용 start");
    }
}
