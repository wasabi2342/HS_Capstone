using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonNetworkConnectManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkConnectManager Instance { get; private set; }

    private string gameVersion = "1";

    private GameObject photonNetworkManager = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("포톤 마스터 서버에 연결됨");

        if (!PhotonNetwork.OfflineMode)  // 온라인일 경우에만 로비 접속
        {
            PhotonNetwork.JoinLobby();
            Debug.Log("포톤 로비 접속 시도");
        }
    }

    public void ConnectPhoton()
    {
        PhotonNetwork.OfflineMode = false; // 온라인 모드
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("포톤서버 접속 시도 (온라인)");
    }

    public void ConnectPhotonToSinglePlay()
    {
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.GameVersion = gameVersion;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;

        PhotonNetwork.CreateRoom("싱글플레이", roomOptions);

        Debug.Log("싱글 플레이 (오프라인 모드)로 포톤 접속 및 방 생성");
    }

    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.OfflineMode)
            UIManager.Instance.OpenPanelInOverlayCanvas<UIRoomPanel>();
        if (PhotonNetwork.IsMasterClient)
            photonNetworkManager = PhotonNetwork.Instantiate("PhotonNetworkManager", Vector3.zero, Quaternion.identity);
    }

    public override void OnLeftRoom()
    {
        if (photonNetworkManager != null)
            Destroy(photonNetworkManager);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText(message);
    }

    public void CreateRoom(string roomName, RoomOptions roomOptions)
    {
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }
}
