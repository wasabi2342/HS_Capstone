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
    private Dictionary<int, int> rewardVotes = new Dictionary<int, int>();


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
            PhotonNetwork.LoadLevel("Level0");

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
            PhotonNetwork.LoadLevel("Level0");
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

    //[PunRPC]
    //public void UpdatePlayerDic(int actNum, int viewID)
    //{
    //    PhotonView targetView = PhotonView.Find(viewID);
    //    if (targetView != null)
    //    {
    //        //RoomManager.Instance.AddPlayerDic(actNum, targetView.gameObject);
    //        //RoomManager.Instance.UpdateSortedPlayers();
    //    }
    //}

    //public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    //{
    //    foreach (var player in RoomManager.Instance.players)
    //    {
    //        int actNum = player.Key;
    //        int viewID = player.Value.GetComponent<PhotonView>().ViewID;
    //        photonView.RPC("UpdatePlayerDic", newPlayer, actNum, viewID);
    //    }
    //}
    [PunRPC]
    public void RPC_OpenRewardUIForAll()
    {
        // 중앙 UIManager를 통해 UIRewardPanel 프리팹을 Instantiate
        UIManager.Instance.OpenPopupPanel<UIRewardPanel>();
    }
    [PunRPC]
    public void RPC_OpenUI()
    {
        if (UIRewardPanel.Instance != null && UIRewardPanel.Instance.rewardUI != null)
        {
            UIRewardPanel.Instance.rewardUI.SetActive(true);
        }
    }

    [PunRPC]
    public void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        if (rewardVotes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }
        rewardVotes[actorNum] = rewardIndex;
        // 모든 클라이언트에서 해당 플레이어의 체크 아이콘 추가
        photonView.RPC("RPC_AddCheckMark", RpcTarget.All, actorNum, rewardIndex);
    }

    // [All Clients] 해당 플레이어(actorNum)가 투표한 보상(rewardIndex)에 대해 체크 아이콘 추가
    [PunRPC]
    public void RPC_AddCheckMark(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.AddCheckMark(rewardIndex, actorNum);
        }
    }

    [PunRPC]
    public void RPC_RemoveMyVote(int actorNum)
    {
        if (rewardVotes.ContainsKey(actorNum))
        {
            int rewardIndex = rewardVotes[actorNum];
            rewardVotes.Remove(actorNum);
            // 모든 클라이언트에 해당 플레이어의 체크 아이콘 제거 요청
            photonView.RPC("RPC_RemoveMyCheckIcon", RpcTarget.All, actorNum, rewardIndex);
        }
        else
        {
            Debug.Log($"[RPC_RemoveMyVote] player {actorNum} did not vote or already removed.");
        }
    }

    [PunRPC]
    public void RPC_RemoveMyCheckIcon(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.RemoveMyCheckIcons(rewardIndex, actorNum);
        }
    }


    /// 최종 보상 확정 RPC: 각 보상에 대해 투표한 인원 수에 기반하여 당첨 확률을 계산하고, weighted random으로 당첨 보상을 결정
    [PunRPC]
    public void RPC_FinalizeRewardSelection()
    {
        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int rewardCount = UIRewardPanel.Instance.rewardDatas.Length;
        float[] voteCounts = new float[rewardCount];

        // 각 옵션 별 투표 수 계산
        foreach (var kvp in rewardVotes)
        {
            int rIndex = kvp.Value;
            if (rIndex >= 0 && rIndex < rewardCount)
            {
                voteCounts[rIndex] += 1f;
            }
        }

        // 가중치로 확률 계산 (비율)
        float[] probabilities = new float[rewardCount];
        for (int i = 0; i < rewardCount; i++)
        {
            probabilities[i] = voteCounts[i] / totalPlayers;  // 만약 투표자가 없다면 0
        }

        // weighted random selection
        float r = UnityEngine.Random.Range(0f, 1f);
        float cumulative = 0f;
        int winningIndex = 0;
        for (int i = 0; i < rewardCount; i++)
        {
            cumulative += probabilities[i];
            if (r < cumulative)
            {
                winningIndex = i;
                break;
            }
        }

        string winningRewardMessage = $"당첨 보상: {UIRewardPanel.Instance.rewardDatas[winningIndex].rewardDetail}";
        Debug.Log($"최종 선택된 보상 인덱스: {winningIndex} (확률: {probabilities[winningIndex]:0.00}), 랜덤값: {r}");

        // 모든 클라이언트에 최종 당첨 결과 업데이트 RPC 호출
        photonView.RPC("RPC_UpdateFinalReward", RpcTarget.All, winningIndex, winningRewardMessage);
    }

  /// 최종 당첨 결과 업데이트: RewardName 텍스트를 지우고 Detail 텍스트에 당첨 결과 표시

    [PunRPC]
    public void RPC_UpdateFinalReward(int winningIndex, string rewardMessage)
    {
        if (UIRewardPanel.Instance != null)
        {
            if (UIRewardPanel.Instance.rewardNameText != null)
                UIRewardPanel.Instance.rewardNameText.text = "";
            if (UIRewardPanel.Instance.detailText != null)
                UIRewardPanel.Instance.detailText.text = rewardMessage;
        }
    }
}
