using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviour
{
    void Awake()
    {
        // ��: �ʴ� 30ȸ ����, 15ȸ ���µ���ȭ
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 15;
    }
}
