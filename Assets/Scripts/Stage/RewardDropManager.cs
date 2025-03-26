using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace Assets.Scripts.Stage
{
    public class RewardDropManager : MonoBehaviourPun
    {
        // Inspector에 A 보상, B 보상 프리팹 할당
        public GameObject aRewardPrefab;
        public GameObject bRewardPrefab;
        // 보상 오브젝트가 드랍될 위치
        public Transform rewardDropPoint;

        void Start()
        {
            if (RewardData.SelectedRewardType.HasValue)
            {
                if (RewardData.SelectedRewardType.Value == RewardType.A)
                {
                    PhotonNetwork.Instantiate(aRewardPrefab.name, rewardDropPoint.position, rewardDropPoint.rotation);
                    Debug.Log("Dropped reward A.");
                }
                else if (RewardData.SelectedRewardType.Value == RewardType.B)
                {
                    PhotonNetwork.Instantiate(bRewardPrefab.name, rewardDropPoint.position, rewardDropPoint.rotation);
                    Debug.Log("Dropped reward B.");
                }
            }
            // 보상 정보 초기화
            RewardData.SelectedRewardType = null;
        }
    }

}