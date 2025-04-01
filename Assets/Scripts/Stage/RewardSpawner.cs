using UnityEngine;
using Photon.Pun;

public class RewardSpawner : MonoBehaviourPun
{
    public string rewardAPrefabName = "RewardA";
    public string rewardBPrefabName = "RewardB";

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        TrySpawnReward();
    }

    private void TrySpawnReward()
    {
        // 예: 투표 결과값 가져오기
        var result = (string)PhotonNetwork.CurrentRoom.CustomProperties["VoteResult"];

        if (result == "A")
        {
            PhotonNetwork.Instantiate(rewardAPrefabName, Vector3.zero, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(rewardBPrefabName, Vector3.zero, Quaternion.identity);
        }
    }
}
