using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Photon 서버에 연결
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 마스터 서버에 연결됨");
        // 랜덤 룸에 입장 시도
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 룸 입장 실패, 새 룸 생성");
        // 4명 방 생성
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("룸에 입장함");

        // UnityEngine.Random 을 명시적으로 사용 (System.Random과 구분)
        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(-2f, 2f),
            0f,
            UnityEngine.Random.Range(-2f, 2f)
        );

        
        PhotonNetwork.Instantiate("Resources_esther/Player", spawnPosition, Quaternion.identity);
    }
} 
