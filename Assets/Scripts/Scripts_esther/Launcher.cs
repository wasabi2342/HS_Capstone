using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Photon ����
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

        // Player ������ ���� ���� (Assets/Resources/Resource_esther/Player.prefab �̶�� ����)
        PhotonNetwork.Instantiate("Resource_esther/Player", Vector3.zero, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"�� �÷��̾� ����: {newPlayer.NickName}, ���� �ο�: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
}
