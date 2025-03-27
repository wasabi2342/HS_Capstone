using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;

public enum RewardType { A, B }

public class Door : MonoBehaviourPun
{
    // 상호작용한 플레이어 목록(ActorNumber를 저장)
    private HashSet<int> interactedPlayers = new HashSet<int>();

    public int interactionCount = 0;

    [SerializeField] private TMP_Text countText;
    public RewardType rewardType;
    [SerializeField] private TMP_Text rewardText;

    private void Start()
    {
        UpdateRewardText();
    }

    private void UpdateRewardText()
    {
        if (rewardText != null)
        {
            rewardText.text = $"Reward: {rewardType}";
        }
    }

    public void Interact()
    {
        if (!photonView.IsMine) return;

        // 로컬 플레이어의 ActorNumber를 인자로 넘김
        photonView.RPC("RPC_Interact", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_Interact(int actorNumber)
    {
        // 이미 상호작용한 플레이어인지 확인
        if (interactedPlayers.Contains(actorNumber))
            return;

        // 첫 상호작용이면 목록에 추가
        interactedPlayers.Add(actorNumber);

        // 상호작용 횟수 증가
        interactionCount++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (countText != null)
            countText.text = interactionCount.ToString();
    }
}
