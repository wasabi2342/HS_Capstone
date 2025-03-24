using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviourPun, IInteractable
{
    // 부활 대상 감지를 위한 변수들
    private bool isInReviveRange = false;
    private WhitePlayerController stunnedPlayer;
    private Coroutine reviveCoroutine;

    // 트리거 진입: 기절한 플레이어 감지
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

                // 기절한 플레이어의 부활 캔버스와 게이지 표시
                stunnedPlayer.reviveCanvas.gameObject.SetActive(true);
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // 트리거 이탈: 범위에서 벗어나면 취소
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

    // IInteractable 인터페이스 구현: F키 등 입력 이벤트 처리
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!photonView.IsMine) return;
        if (!isInReviveRange || stunnedPlayer == null) return;

        // 입력 상태에 따라 처리:
        if (ctx.started && reviveCoroutine == null)
        {
            // F키 눌림 시작 시 부활 게이지 채우기 코루틴 시작
            reviveCoroutine = StartCoroutine(ReviveGaugeRoutine());
        }
        else if (ctx.canceled && reviveCoroutine != null)
        {
            // F키에서 손을 뗐을 때 코루틴 취소
            StopCoroutine(reviveCoroutine);
            reviveCoroutine = null;
            stunnedPlayer.reviveGauge.fillAmount = 0;
        }
        // 만약 ctx.performed 를 별도로 처리할 필요가 있다면 여기에 추가 가능
    }

    // 부활 게이지를 채우는 코루틴
    private IEnumerator ReviveGaugeRoutine()
    {
        float timer = 0f;
        stunnedPlayer.reviveGauge.fillAmount = 0;

        while (timer < 3f)
        {
            // 범위에서 벗어나거나 상태가 변경되면 취소
            if (!isInReviveRange || stunnedPlayer == null || stunnedPlayer.currentState != WhitePlayerState.Stun)
            {
                stunnedPlayer.reviveGauge.fillAmount = 0;
                yield break;
            }

            timer += Time.deltaTime;
            stunnedPlayer.reviveGauge.fillAmount = timer / 3f;
            yield return null;
        }

        // 3초간 유지 성공 시 부활 RPC 호출
        if (stunnedPlayer != null && stunnedPlayer.currentState == WhitePlayerState.Stun)
        {
            stunnedPlayer.photonView.RPC("ReviveRPC", RpcTarget.MasterClient);
            stunnedPlayer.reviveCanvas.gameObject.SetActive(false);
        }
        reviveCoroutine = null;
    }
}
