using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : GaugeInteraction
{
    private WhitePlayerController whitePlayer;
    private PhotonView otherPhotonView;
    private PhotonView localPhotonView;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();
        localPhotonView = whitePlayer.GetComponent<PhotonView>();
    }

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        if (whitePlayer.currentState != WhitePlayerState.Stun || otherPhotonView == null)
        {
            return;
        }

        // 같은 팀일 때만 부활 상호작용
        if (IsSameTeam(localPhotonView, otherPhotonView))
        {
            base.OnInteract(ctx);
        }
        //whitePlayer.HandleReviveInteraction(ctx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(whitePlayer.currentState != WhitePlayerState.Stun)
        {
            return;
        }

        PhotonView otherView = other.GetComponentInParent<PhotonView>();
        if (otherView.IsMine)
            otherPhotonView = otherView;

        Debug.Log("부활 상호작용 enter");

        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (whitePlayer.currentState != WhitePlayerState.Stun)
        {
            return;
        }

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
    private bool IsSameTeam(PhotonView localView, PhotonView otherView)
    {
        if (!TryGetTeamId(localView, out int myTeamId) || !TryGetTeamId(otherView, out int otherTeamId))
        {
            Debug.Log("TeamId가 설정되지 않았습니다.");
            return false;
        }

        return myTeamId == otherTeamId;
    }

    private bool TryGetTeamId(PhotonView view, out int teamId)
    {
        if (view.Owner.CustomProperties.TryGetValue("TeamId", out object value))
        {
            teamId = (int)value;
            return true;
        }
        teamId = -1;
        return false;
    }
}
