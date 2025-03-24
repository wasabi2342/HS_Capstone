using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviourPun, IInteractable
{
    // �ڽ��� WhitePlayerController (�� ��ũ��Ʈ�� ���� ������Ʈ �Ǵ� �θ� �ִٰ� ����)
    private WhitePlayerController whitePlayer;

    // ��Ȱ ��� ����
    private bool isInReviveRange = false;
    private WhitePlayerController stunnedPlayer;
    private Coroutine reviveCoroutine;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();
    }

    // Ʈ���� ����
    private void OnTriggerEnter(Collider other)
    {
        // ���� ������ ĳ���Ͱ� �ƴϸ� ����
        if (!photonView.IsMine) return;

        // Interactable ���̾� �浹 ��
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            WhitePlayerController otherPlayer = other.GetComponentInParent<WhitePlayerController>();
            if (otherPlayer != null && otherPlayer.currentState == WhitePlayerState.Stun)
            {
                isInReviveRange = true;
                stunnedPlayer = otherPlayer;

                // ���(������)�� Canvas�� Gauge ǥ��
                stunnedPlayer.reviveCanvas.gameObject.SetActive(true);
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // Ʈ���� ��Ż
    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            WhitePlayerController otherPlayer = other.GetComponentInParent<WhitePlayerController>();
            if (otherPlayer != null && stunnedPlayer == otherPlayer)
            {
                isInReviveRange = false;

                stunnedPlayer.reviveCanvas.gameObject.SetActive(false);
                stunnedPlayer.reviveGauge.fillAmount = 0;
                stunnedPlayer = null;

                if (reviveCoroutine != null)
                {
                    StopCoroutine(reviveCoroutine);
                    reviveCoroutine = null;
                }
            }
        }
    }

    // FŰ �� ��ȣ�ۿ� �Է� (IInteractable �������̽�)
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!photonView.IsMine) return;

        // ���� ���� ��밡 ���� �ȿ� �ִٸ�
        if (isInReviveRange && stunnedPlayer != null)
        {
            // FŰ ������ ����
            if (ctx.started && reviveCoroutine == null)
            {
                reviveCoroutine = StartCoroutine(ReviveGaugeRoutine());
            }
            // FŰ ������ ����
            else if (ctx.canceled && reviveCoroutine != null)
            {
                StopCoroutine(reviveCoroutine);
                reviveCoroutine = null;
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // ���� ��Ȱ �������� ä��� �ڷ�ƾ
    private IEnumerator ReviveGaugeRoutine()
    {
        float timer = 0f;
        stunnedPlayer.reviveGauge.fillAmount = 0;

        while (timer < 3f)
        {
            // �߰��� ������ Ǯ���ų�(Death ��), �������� ����� ���
            if (!isInReviveRange || stunnedPlayer == null || stunnedPlayer.currentState != WhitePlayerState.Stun)
            {
                stunnedPlayer.reviveGauge.fillAmount = 0;
                yield break;
            }

            timer += Time.deltaTime;
            stunnedPlayer.reviveGauge.fillAmount = timer / 3f;
            yield return null;
        }

        // 3�ʰ� ���� ���� -> ��Ȱ RPC ȣ��
        if (stunnedPlayer != null && stunnedPlayer.currentState == WhitePlayerState.Stun)
        {
            stunnedPlayer.photonView.RPC("ReviveRPC", RpcTarget.MasterClient);
            stunnedPlayer.reviveCanvas.gameObject.SetActive(false);
        }

        reviveCoroutine = null;
    }
}
