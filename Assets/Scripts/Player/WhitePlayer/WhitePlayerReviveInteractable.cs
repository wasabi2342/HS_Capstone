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

        // ���� ���� ���� ��Ȱ ��ȣ�ۿ�
        if (IsSameTeam(localPhotonView, otherPhotonView))
        {
            base.OnInteract(ctx);
        }
        //whitePlayer.HandleReviveInteraction(ctx);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (whitePlayer.currentState != WhitePlayerState.Stun)
        {
            return;
        }

        PhotonView otherView = other.GetComponentInParent<PhotonView>();
        if (otherView.IsMine)
            otherPhotonView = otherView;

        Debug.Log("��Ȱ ��ȣ�ۿ� enter");
        if (IsSameTeam(localPhotonView, otherPhotonView))
        {
            base.OnTriggerEnter(other);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (whitePlayer.currentState != WhitePlayerState.Stun)
        {
            return;
        }
        if (IsSameTeam(localPhotonView, otherPhotonView))
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
