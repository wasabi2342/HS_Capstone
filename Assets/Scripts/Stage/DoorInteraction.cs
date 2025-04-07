using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private bool canInteract = true;
    [SerializeField] private Canvas canvas; // 문 주변에 "F키 상호작용" 안내 표시할 Canvas

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started && canInteract)
        {
            canInteract = false;
            // 문과 상호작용 → Reward UI 열기
            // 모든 플레이어가 RewardCanvas를 보도록 RPC or static 호출
            RewardManager.Instance.OpenRewardUI();

            // 입력 맵 변경 등 (사용자 프로젝트 구조에 맞게)
            // InputManager.Instance.ChangeDefaultMap("UI");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 내 로컬 플레이어만 상호작용 가능
        if (other.CompareTag("Player") &&
            other.GetComponentInParent<PhotonView>()?.IsMine == true)
        {
            if (canInteract && canvas != null)
                canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") &&
            other.GetComponentInParent<PhotonView>()?.IsMine == true)
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }
    }
}
