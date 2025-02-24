using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviour
{
    void Awake()
    {
        // 예: 초당 30회 전송, 15회 상태동기화
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 15;
    }
}
