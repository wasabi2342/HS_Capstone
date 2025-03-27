using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    private string gameVersion = "1";

    public event Action<List<RoomInfo>> OnRoomListUpdated;
    public event Action<int> OnUpdateReadyPlayer;

    private Dictionary<int, bool> readyPlayers = new Dictionary<int, bool>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        Debug.Log("포톤서버 로비 접속");
    }

    public void ConnectPhoton()
    {
        if (PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("포톤서버 접속");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated?.Invoke(roomList);
        Debug.Log("방 갱신");
    }

    public override void OnJoinedRoom()
    {
        UIManager.Instance.CloseAllUI();
        PhotonNetwork.LoadLevel("Room");
        //gameObject.AddComponent<RoomManager>();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText(message);
    }

    public void CreateRoom(string roomName, RoomOptions roomOptions)
    {
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void ReadyToEnterStage()
    {
        if (!RoomManager.Instance.isEnteringStage)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC("OpenReadyPanel", RpcTarget.All);
            }
            else
            {
                RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData();
                SceneManager.LoadScene("StageTest1");
            }
        }
        if (PhotonNetwork.InRoom)
            photonView.RPC("UpdateReadyPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private Coroutine stageEnterCoroutine;

    [PunRPC]
    private void OpenReadyPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            stageEnterCoroutine = StartCoroutine(TimeCount());
        }
        RoomManager.Instance.isEnteringStage = true;
        UIStageReadyPanel panel = UIManager.Instance.OpenPopupPanel<UIStageReadyPanel>();
        panel.Init();
        OnUpdateReadyPlayer += panel.UpdateToggls;
    }

    private IEnumerator TimeCount()
    {
        yield return new WaitForSeconds(60);
        Debug.Log("60초 경과 스테이지 진입");
        if (PhotonNetwork.IsMasterClient)
        {

            PhotonNetwork.CurrentRoom.IsOpen = false;
            RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData();
            PhotonNetwork.LoadLevel("StageTest1");

        }
    }

    [PunRPC]
    private void UpdateReadyPlayer(int playerNum)
    {
        if (!readyPlayers.ContainsKey(playerNum))
        {
            readyPlayers.Add(playerNum, true);
            OnUpdateReadyPlayer.Invoke(readyPlayers.Count);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == readyPlayers.Count && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("모두 준비 완료");
            StopCoroutine(stageEnterCoroutine);
            UIManager.Instance.ClosePeekUI();
            RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData();

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("StageTest1");
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (stageEnterCoroutine != null)
        {
            StopCoroutine(stageEnterCoroutine);
        }
        if (UIManager.Instance.ReturnPeekUI() as UIStageReadyPanel)
        {
            UIManager.Instance.ClosePeekUI();
            readyPlayers.Clear();
        }
        if (RoomManager.Instance.players.ContainsKey(otherPlayer.ActorNumber))
        {
            RoomManager.Instance.players.Remove(otherPlayer.ActorNumber);
            RoomManager.Instance.UpdateSortedPlayers();
        }
    }

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public void AddPlayer(int actNum, int viewID)
    {
        photonView.RPC("UpdatePlayerDic", RpcTarget.Others, actNum, viewID);
    }

    [PunRPC]
    public void UpdatePlayerDic(int actNum, int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            RoomManager.Instance.AddPlayerDic(actNum, targetView.gameObject);
            RoomManager.Instance.UpdateSortedPlayers();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        foreach (var player in RoomManager.Instance.players)
        {
            int actNum = player.Key;
            int viewID = player.Value.GetComponent<PhotonView>().ViewID;
            photonView.RPC("UpdatePlayerDic", newPlayer, actNum, viewID);
        }
    }
}
