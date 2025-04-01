using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviourPun
{
    private bool isInteracted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isInteracted) return;
        if (!other.CompareTag("Player")) return;

        isInteracted = true;
        // 문과 상호작용 발생 → 모든 클라이언트에서 투표 UI 표시
        photonView.RPC("RPC_ShowVoteUI", RpcTarget.All);
    }

    [PunRPC]
    void RPC_ShowVoteUI()
    {
        // 모든 클라이언트에서 투표 UI 열기
        // 예: VoteManager 또는 RewardVoteUI
        if (VoteManager.Instance != null)
        {
            VoteManager.Instance.ShowVoteUI();
            Debug.Log("RPC_ShowVoteUI 호출됨");
        }
        else
        {
            Debug.LogWarning("VoteManager Instance가 존재하지 않습니다!");
        }
    }
}
