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

        Debug.Log("��Ȱ ��ȣ�ۿ� enter");

        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        Debug.Log("��Ȱ ��ȣ�ۿ� exit");
    }

    protected override void OnPerformedEvent()
    {
        base.OnPerformedEvent();
        
        Debug.Log("��Ȱ ��ȣ�ۿ� ");

        whitePlayer.Revive();
    }

    protected override void OnCanceledEvent()
    {
        base.OnCanceledEvent();
        
        Debug.Log("��Ȱ ��ȣ�ۿ� cancel");
    }

    protected override void OnStartedEvent()
    {
        base.OnStartedEvent();

        Debug.Log("��Ȱ ��ȣ�ۿ� start");
    }
}
