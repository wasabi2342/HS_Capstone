using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinkPlayerReviveInteractable : GaugeInteraction
{
    private PinkPlayerController pinkPlayer;
    private PhotonView otherPhotonView;
    private PhotonView localPhotonView;

    private void Awake()
    {
        pinkPlayer = GetComponentInParent<PinkPlayerController>();
        localPhotonView = pinkPlayer.GetComponent<PhotonView>();
    }

    public override void OnInteract(InputAction.CallbackContext ctx)
    {
        if (pinkPlayer.currentState != PinkPlayerState.Stun || otherPhotonView == null)
        {
            return;
        }

        // ���� ���� ���� ��Ȱ ��ȣ�ۿ�
        if (IsSameTeam(localPhotonView, otherPhotonView))
        {
            base.OnInteract(ctx);
        }
        //whitePlayer.HandleReviveInteraction(ctx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (pinkPlayer.currentState != PinkPlayerState.Stun)
        {
            return;
        }

        PhotonView otherView = other.GetComponentInParent<PhotonView>();
        if (otherView.IsMine)
            otherPhotonView = otherView;

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

    private bool IsSameTeam(PhotonView localView, PhotonView otherView)
    {
        // TeamId�� �������µ� �����ϸ� �⺻������ -1�� ����ϰ�, �⺻������ ���� ������ ó��
        if (!TryGetTeamId(localView, out int myTeamId))
        {
            myTeamId = -1; // TeamId�� ������ �⺻�� -1
        }

        if (!TryGetTeamId(otherView, out int otherTeamId))
        {
            otherTeamId = -1; // TeamId�� ������ �⺻�� -1
        }

        // TeamId�� �������� �ʾҰų� ���� ���� �� true ��ȯ
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
