using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Photon 연결
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
        Debug.Log($"방 입장: {PhotonNetwork.CurrentRoom.Name}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Player 프리팹 동적 생성 (Assets/Resources/Resource_esther/Player.prefab 이라고 가정)
        PhotonNetwork.Instantiate("Resource_esther/Player", Vector3.zero, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"새 플레이어 입장: {newPlayer.NickName}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
}
