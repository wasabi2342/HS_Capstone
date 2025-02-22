using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to MasterServer");
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("MyFixedRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"�� ����: {PhotonNetwork.CurrentRoom.Name}, ���� �ο�: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // (1) Player �������� �ٴ� ���� ��¦ �÷��� ����
        Vector3 spawnPos = new Vector3(0, 1f, 0); // y=1 ������ ����
        PhotonNetwork.Instantiate("Resource_esther/Player", spawnPos, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"�� �÷��̾� ����: {newPlayer.NickName}, ���� �ο�: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
}
