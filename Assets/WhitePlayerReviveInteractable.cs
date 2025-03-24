using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviourPun, IInteractable
{
    // ��Ȱ ��� ������ ���� ������
    private bool isInReviveRange = false;
    private WhitePlayerController stunnedPlayer;
    private Coroutine reviveCoroutine;

    // Ʈ���� ����: ������ �÷��̾� ����
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            WhitePlayerController otherPlayer = other.GetComponentInParent<WhitePlayerController>();
            if (otherPlayer != null && otherPlayer.currentState == WhitePlayerState.Stun)
            {
                isInReviveRange = true;
                stunnedPlayer = otherPlayer;

                // ������ �÷��̾��� ��Ȱ ĵ������ ������ ǥ��
                stunnedPlayer.reviveCanvas.gameObject.SetActive(true);
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // Ʈ���� ��Ż: �������� ����� ���
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

    // IInteractable �������̽� ����: FŰ �� �Է� �̺�Ʈ ó��
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!photonView.IsMine) return;
        if (!isInReviveRange || stunnedPlayer == null) return;

        // �Է� ���¿� ���� ó��:
        if (ctx.started && reviveCoroutine == null)
        {
            // FŰ ���� ���� �� ��Ȱ ������ ä��� �ڷ�ƾ ����
            reviveCoroutine = StartCoroutine(ReviveGaugeRoutine());
        }
        else if (ctx.canceled && reviveCoroutine != null)
        {
            // FŰ���� ���� ���� �� �ڷ�ƾ ���
            StopCoroutine(reviveCoroutine);
            reviveCoroutine = null;
            stunnedPlayer.reviveGauge.fillAmount = 0;
        }
        // ���� ctx.performed �� ������ ó���� �ʿ䰡 �ִٸ� ���⿡ �߰� ����
    }

    // ��Ȱ �������� ä��� �ڷ�ƾ
    private IEnumerator ReviveGaugeRoutine()
    {
        float timer = 0f;
        stunnedPlayer.reviveGauge.fillAmount = 0;

        while (timer < 3f)
        {
            // �������� ����ų� ���°� ����Ǹ� ���
            if (!isInReviveRange || stunnedPlayer == null || stunnedPlayer.currentState != WhitePlayerState.Stun)
            {
                stunnedPlayer.reviveGauge.fillAmount = 0;
                yield break;
            }

            timer += Time.deltaTime;
            stunnedPlayer.reviveGauge.fillAmount = timer / 3f;
            yield return null;
        }

        // 3�ʰ� ���� ���� �� ��Ȱ RPC ȣ��
        if (stunnedPlayer != null && stunnedPlayer.currentState == WhitePlayerState.Stun)
        {
            stunnedPlayer.photonView.RPC("ReviveRPC", RpcTarget.MasterClient);
            stunnedPlayer.reviveCanvas.gameObject.SetActive(false);
        }
        reviveCoroutine = null;
    }
}
