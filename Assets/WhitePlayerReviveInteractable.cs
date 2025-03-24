using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhitePlayerReviveInteractable : MonoBehaviourPun, IInteractable
{
    // 자신의 WhitePlayerController (이 스크립트가 같은 오브젝트 또는 부모에 있다고 가정)
    private WhitePlayerController whitePlayer;

    // 부활 대상 정보
    private bool isInReviveRange = false;
    private WhitePlayerController stunnedPlayer;
    private Coroutine reviveCoroutine;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();
    }

    // 트리거 진입
    private void OnTriggerEnter(Collider other)
    {
        // 내가 소유한 캐릭터가 아니면 리턴
        if (!photonView.IsMine) return;

        // Interactable 레이어 충돌 시
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            WhitePlayerController otherPlayer = other.GetComponentInParent<WhitePlayerController>();
            if (otherPlayer != null && otherPlayer.currentState == WhitePlayerState.Stun)
            {
                isInReviveRange = true;
                stunnedPlayer = otherPlayer;

                // 상대(기절자)의 Canvas와 Gauge 표시
                stunnedPlayer.reviveCanvas.gameObject.SetActive(true);
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // 트리거 이탈
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

    // F키 등 상호작용 입력 (IInteractable 인터페이스)
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!photonView.IsMine) return;

        // 기절 중인 상대가 범위 안에 있다면
        if (isInReviveRange && stunnedPlayer != null)
        {
            // F키 누르기 시작
            if (ctx.started && reviveCoroutine == null)
            {
                reviveCoroutine = StartCoroutine(ReviveGaugeRoutine());
            }
            // F키 누르기 해제
            else if (ctx.canceled && reviveCoroutine != null)
            {
                StopCoroutine(reviveCoroutine);
                reviveCoroutine = null;
                stunnedPlayer.reviveGauge.fillAmount = 0;
            }
        }
    }

    // 실제 부활 게이지를 채우는 코루틴
    private IEnumerator ReviveGaugeRoutine()
    {
        float timer = 0f;
        stunnedPlayer.reviveGauge.fillAmount = 0;

        while (timer < 3f)
        {
            // 중간에 기절이 풀리거나(Death 등), 범위에서 벗어나면 취소
            if (!isInReviveRange || stunnedPlayer == null || stunnedPlayer.currentState != WhitePlayerState.Stun)
            {
                stunnedPlayer.reviveGauge.fillAmount = 0;
                yield break;
            }

            timer += Time.deltaTime;
            stunnedPlayer.reviveGauge.fillAmount = timer / 3f;
            yield return null;
        }

        // 3초간 유지 성공 -> 부활 RPC 호출
        if (stunnedPlayer != null && stunnedPlayer.currentState == WhitePlayerState.Stun)
        {
            stunnedPlayer.photonView.RPC("ReviveRPC", RpcTarget.MasterClient);
            stunnedPlayer.reviveCanvas.gameObject.SetActive(false);
        }

        reviveCoroutine = null;
    }
}
