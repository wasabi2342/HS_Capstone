using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Door door; // 같은 GameObject나 자식에 Door 스크립트가 있다면 할당
    private bool isPlayerInRange = false;

    void Awake()
    {
        if (door == null)
            door = GetComponent<Door>();
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어가 범위에 들어옴
        if (other.CompareTag("Player"))
        {
            // PhotonView.IsMine으로 로컬 플레이어만 UI 표시 가능
            PhotonView pv = other.GetComponentInParent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                isPlayerInRange = true;
                // 상호작용 UI 표시 로직 (optional)
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 플레이어가 범위를 벗어남
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.GetComponentInParent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                isPlayerInRange = false;
                // 상호작용 UI 비활성화 로직 (optional)
            }
        }
    }

    // IInteractable 구현
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started && isPlayerInRange)
        {
            // 트리거 범위 안에 있는 로컬 플레이어가 Interact 키를 누름
            door.Interact();
        }
    }
}
