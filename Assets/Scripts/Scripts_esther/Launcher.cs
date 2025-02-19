using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Photon ������ ����
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon ������ ������ �����");
        // ���� �뿡 ���� �õ�
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ����, �� �� ����");
        // 4�� �� ����
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�뿡 ������");

        // UnityEngine.Random �� ��������� ��� (System.Random�� ����)
        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(-2f, 2f),
            0f,
            UnityEngine.Random.Range(-2f, 2f)
        );

        
        PhotonNetwork.Instantiate("Resources_esther/Player", spawnPosition, Quaternion.identity);
    }
} 
