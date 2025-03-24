using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer; // 기절된 대상
    private Coroutine reviveCoroutine;

    private Image reviveProgressPanel;
    private Image reviveProgressBar;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();

        // UI 초기화 (플레이어 Canvas에 미리 배치)
        reviveProgressPanel = GameObject.Find("ReviveProgressPanel").GetComponent<Image>();
        reviveProgressBar = GameObject.Find("ReviveProgressBar").GetComponent<Image>();

        reviveProgressPanel.gameObject.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (whitePlayer.currentState != WhitePlayerState.Stun)
            return;

        if (ctx.started)
        {
            reviveCoroutine = StartCoroutine(ReviveProgress());
        }
        else if (ctx.canceled)
        {
            if (reviveCoroutine != null)
                StopCoroutine(reviveCoroutine);

            ResetReviveUI();
        }
    }

    IEnumerator ReviveProgress()
    {
        float holdDuration = 3f;
        float elapsed = 0f;

        reviveProgressPanel.gameObject.SetActive(true);
        reviveProgressBar.fillAmount = 0f;

        // 진행도 UI가 기절한 플레이어 머리 위에 있도록 (월드스페이스 사용 시 권장)
        while (elapsed < holdDuration)
        {
            if (whitePlayer.currentState != WhitePlayerState.Stun)
            {
                ResetReviveUI();
                yield break;
            }

            elapsed += Time.deltaTime;
            reviveProgressBar.fillAmount = elapsed / holdDuration;

            yield return null;
        }

        // 부활 실행
        whitePlayer.Revive();
        Debug.Log("플레이어 부활 완료!");

        ResetReviveUI();
    }

    void ResetReviveUI()
    {
        reviveProgressBar.fillAmount = 0f;
        reviveProgressPanel.gameObject.SetActive(false);
    }
}
