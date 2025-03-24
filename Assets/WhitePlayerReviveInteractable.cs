using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer; // ������ ���
    private Coroutine reviveCoroutine;

    private Image reviveProgressPanel;
    private Image reviveProgressBar;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();

        // UI �ʱ�ȭ (�÷��̾� Canvas�� �̸� ��ġ)
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

        // ���൵ UI�� ������ �÷��̾� �Ӹ� ���� �ֵ��� (���彺���̽� ��� �� ����)
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

        // ��Ȱ ����
        whitePlayer.Revive();
        Debug.Log("�÷��̾� ��Ȱ �Ϸ�!");

        ResetReviveUI();
    }

    void ResetReviveUI()
    {
        reviveProgressBar.fillAmount = 0f;
        reviveProgressPanel.gameObject.SetActive(false);
    }
}
