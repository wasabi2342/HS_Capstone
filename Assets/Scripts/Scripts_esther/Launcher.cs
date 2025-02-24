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
        Debug.Log($"방 입장: {PhotonNetwork.CurrentRoom.Name}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // (1) Player 프리팹을 바닥 위로 살짝 올려서 스폰
        Vector3 spawnPos = new Vector3(0, 1f, 0); // y=1 정도로 높임
        PhotonNetwork.Instantiate("Resource_esther/Player", spawnPos, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"새 플레이어 입장: {newPlayer.NickName}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
}
