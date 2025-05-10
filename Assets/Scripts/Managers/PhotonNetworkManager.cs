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

    public event Action<int> OnUpdateReadyPlayer;

    private Dictionary<int, bool> readyPlayers = new Dictionary<int, bool>();
    private Dictionary<int, int> rewardVotes = new Dictionary<int, int>();
    private Coroutine finalRewardCountdownCoroutine;
    private Coroutine finalRewardNextStageCoroutine;

    private HashSet<int> deadPlayers = new HashSet<int>();

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
        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("싱글 플레이 모드 - 씬 로드");
            //PhotonNetwork.LoadLevel("room");
            PhotonNetwork.LoadLevel("Tutorial"); // 튜토리얼로
        }
        else
        {
            Debug.Log("멀티플레이 모드 - 씬 로드는 마스터 클라이언트에서 수행해야 함");
            // 예: if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText(message);
    }

    public void CreateRoom(string roomName, RoomOptions roomOptions)
    {
        PhotonNetwork.CreateRoom(roomName, roomOptions);
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
                SceneManager.LoadScene("Level0");
            }
        }
        if (PhotonNetwork.InRoom)
            photonView.RPC("UpdateReadyPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private Coroutine stageEnterCoroutine;

    [PunRPC]
    private void OpenReadyPanel()
    {
        readyPlayers.Clear();
        OnUpdateReadyPlayer = null;

        if (PhotonNetwork.IsMasterClient)
        {
            stageEnterCoroutine = StartCoroutine(TimeCount());
        }
        RoomManager.Instance.isEnteringStage = true;
        UIStageReadyPanel panel = UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIStageReadyPanel>();
        panel.Init();
        OnUpdateReadyPlayer += panel.UpdateToggls;
        OnUpdateReadyPlayer.Invoke(readyPlayers.Count);
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

        if (RoomManager.Instance != null)
        {
            if (RoomManager.Instance.players.ContainsKey(otherPlayer.ActorNumber))
            {
                RoomManager.Instance.players.Remove(otherPlayer.ActorNumber);
                RoomManager.Instance.UpdateSortedPlayers();
            }
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
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIRewardPanel>();
    }
    [PunRPC]
    public void RPC_OpenUI()
    {
        if (UIRewardPanel.Instance != null && UIRewardPanel.Instance.rewardUI != null)
        {
            UIRewardPanel.Instance.rewardUI.SetActive(true);
        }
    }
    /// 투표 확정 RPC: 이미 투표한 플레이어면 무시, 아니라면 기록 후 체크 아이콘 추가
    [PunRPC]
    public void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        if (rewardVotes.ContainsKey(actorNum))
            return;

        rewardVotes[actorNum] = rewardIndex;
        photonView.RPC("RPC_AddCheckMark", RpcTarget.All, actorNum, rewardIndex);

        // 모든 인원이 투표 완료하면 카운트다운 시작 (마스터 클라이언트)
        if (rewardVotes.Count == PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
        {
            finalRewardCountdownCoroutine = StartCoroutine(StartFinalRewardCountdown());
        }
    }

    /// 특정 플레이어(actorNum)가 투표한 보상(rewardIndex)에 대해 체크 아이콘 추가
    [PunRPC]
    public void RPC_AddCheckMark(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.AddCheckMark(rewardIndex, actorNum);
        }
    }

    /// 로컬 플레이어가 취소 요청 시 해당 플레이어의 투표만 제거
    [PunRPC]
    public void RPC_RemoveMyVote(int actorNum)
    {
        if (!rewardVotes.ContainsKey(actorNum))
            return;

        int rewardIndex = rewardVotes[actorNum];
        rewardVotes.Remove(actorNum);

        photonView.RPC("RPC_RemoveMyCheckIcon", RpcTarget.All, actorNum, rewardIndex);

        // 이미 카운트다운이 진행 중이라면 즉시 중단
        if (PhotonNetwork.IsMasterClient && finalRewardCountdownCoroutine != null)
        {
            StopCoroutine(finalRewardCountdownCoroutine);
            finalRewardCountdownCoroutine = null;
        }
    }

    /// 특정 보상에서 해당 플레이어(actorNum)가 추가한 체크 아이콘 제거
    [PunRPC]
    public void RPC_RemoveMyCheckIcon(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.RemoveMyCheckIcons(rewardIndex, actorNum);
        }
    }

    /// 카운트다운 업데이트 RPC: RewardName 텍스트에 남은 초 표시
    [PunRPC]
    public void RPC_UpdateCountdown(int seconds, string unused)
    {
        if (UIRewardPanel.Instance != null && UIRewardPanel.Instance.rewardNameText != null)
        {
            // 남은 시간이 있으면 표시, 없으면 텍스트를 지움
            UIRewardPanel.Instance.rewardNameText.text = seconds > 0 ? seconds.ToString() : "";
        }
    }

    /// 최종 보상 확정 카운트다운 코루틴: 10초부터 0까지 카운트 후 최종 확정 RPC 실행
    private IEnumerator StartFinalRewardCountdown()
    {
        int countdown = 10;
        while (countdown > 0)
        {
            photonView.RPC("RPC_UpdateCountdown", RpcTarget.All, countdown, "");
            yield return new WaitForSeconds(1f);
            countdown--;

            // (선택) 중간에 누군가 투표 취소했으면 여기서도 break
            if (rewardVotes.Count < PhotonNetwork.CurrentRoom.PlayerCount)
                break;
        }

        if (countdown <= 0)
        {
            photonView.RPC("RPC_UpdateCountdown", RpcTarget.All, 0, "");
            photonView.RPC("RPC_FinalizeRewardSelection", RpcTarget.All);
        }

        finalRewardCountdownCoroutine = null;
    }
    ///최종 보상 확정 RPC: weighted random selection 후 결과 전달
    [PunRPC]
    public void RPC_FinalizeRewardSelection()
    {
        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int rewardCount = UIRewardPanel.Instance.rewardDatas.Length;
        float[] voteCounts = new float[rewardCount];
        for (int i = 0; i < rewardCount; i++)
            voteCounts[i] = 0f;

        foreach (var kvp in rewardVotes)
        {
            int index = kvp.Value;
            if (index >= 0 && index < rewardCount)
                voteCounts[index] += 1f;
        }

        float[] probabilities = new float[rewardCount];
        for (int i = 0; i < rewardCount; i++)
        {
            probabilities[i] = totalPlayers > 0 ? voteCounts[i] / totalPlayers : 0f;
        }

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

        Debug.Log($"최종 선택된 보상 인덱스: {winningIndex} (확률: {probabilities[winningIndex]:0.00}), 랜덤값: {r}");
        // 최종 결과는 winningIndex 하나만 전달합니다.
        photonView.RPC("RPC_UpdateFinalReward", RpcTarget.All, winningIndex);

        //photonView.RPC("RPC_SaveRunTimeData", RpcTarget.All); // 게임 데이터 저장

        if (PhotonNetwork.IsMasterClient)
        {
            // 보관할 값을 담을 해시테이블 생성
            var props = new ExitGames.Client.Photon.Hashtable();
            props["FinalRewardIndex"] = winningIndex;

            // 현재 방의 CustomProperties에 세팅
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    /// 최종 당첨 결과 업데이트: RewardName 텍스트는 지우고 Detail 텍스트에 당첨 결과를 표시
    [PunRPC]
    public void RPC_UpdateFinalReward(int winningIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.rewardNameText.text = $"Final Reward: {UIRewardPanel.Instance.rewardDatas[winningIndex].rewardName}";
        }
        if (PhotonNetwork.IsMasterClient && finalRewardNextStageCoroutine == null)
        {
            finalRewardNextStageCoroutine = StartCoroutine(StartNextStageCountdown());
        }
    }

    [PunRPC]
    public void RPC_SaveRunTimeData()
    {
        RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData();
    }

    private IEnumerator StartNextStageCountdown()
    {
        int countdown = 5; // 예) 5초 후 다음 씬 이동
        while (countdown > 0)
        {
            // 모든 클라이언트가 5,4,3,... 메시지를 볼 수 있도록 RPC
            photonView.RPC("RPC_ShowNextStageCountdown", RpcTarget.All, countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // 카운트다운이 끝나면 마지막으로 0초 표기
        photonView.RPC("RPC_ShowNextStageCountdown", RpcTarget.All, 0);
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        // 이후 원하는 씬으로 이동
        PhotonNetwork.LoadLevel("Level1");

        finalRewardNextStageCoroutine = null;
    }
    [PunRPC]
    public void RPC_ShowNextStageCountdown(int seconds)
    {
        if (UIRewardPanel.Instance != null)
        {
            // detailText 아래에 추가로 적거나, 별도의 Text 필드를 만들어서 사용
            if (seconds > 0)
            {
                UIRewardPanel.Instance.detailText.text = $"Going Next Stage... {seconds}s";
            }
            else
            {
                UIRewardPanel.Instance.detailText.text = "Going...";
            }
        }
    }

    [PunRPC]
    public void RPC_ApplyPlayerBuff(float damageBuff)
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().damageBuff *= damageBuff;
        }

        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText($"모두가 협력해 피해량이{damageBuff}배 증가합니다.");
    }

    [PunRPC]
    public void RPC_ApplyMonsterBuff(float damageBuff)
    {
        if (MonsterStatusManager.instance != null)
        {
            MonsterStatusManager.instance.EnemyDamageBuff(damageBuff);
        }

        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText($"모두가 배신해 몬스터의 피해량이{damageBuff}배 증가합니다.");
    }

    [PunRPC]
    public void PopupBlessingPanel()
    {
        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanelInCameraCanvas<UISelectBlessingPanel>();

        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText($"배신에 성공해 가호를 획득합니다.");
    }

    [PunRPC]
    public void PopupDialogPanel(string message)
    {
        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText(message);
    }

    /// <summary>
    /// 플레이어 사망 시 호출
    /// </summary>
    public void ReportPlayerDeath(int actorNumber)
    {
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
        {
            if (!deadPlayers.Contains(actorNumber))
            {
                deadPlayers.Add(actorNumber);
                Debug.Log($"플레이어 {actorNumber} 사망. 현재 사망자 수: {deadPlayers.Count}");

                CheckAllPlayersDead();
            }
        }
        else
        {
            photonView.RPC(nameof(RPC_ReportPlayerDeath), RpcTarget.MasterClient, actorNumber);
        }
    }

    /// <summary>
    /// 플레이어 부활 시 호출
    /// </summary>
    public void ReportPlayerRevive(int actorNumber)
    {
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
        {
            if (deadPlayers.Contains(actorNumber))
            {
                deadPlayers.Remove(actorNumber);
                Debug.Log($"플레이어 {actorNumber} 부활. 현재 사망자 수: {deadPlayers.Count}");
            }
        }
        else
        {
            photonView.RPC(nameof(RPC_ReportPlayerRevive), RpcTarget.MasterClient, actorNumber);
        }
    }

    [PunRPC]
    private void RPC_ReportPlayerDeath(int actorNumber)
    {
        ReportPlayerDeath(actorNumber);
    }

    [PunRPC]
    private void RPC_ReportPlayerRevive(int actorNumber)
    {
        ReportPlayerRevive(actorNumber);
    }

    /// <summary>
    /// 모든 플레이어가 사망했는지 확인
    /// </summary>
    private void CheckAllPlayersDead()
    {
        int totalPlayers = PhotonNetwork.OfflineMode ? 1 : PhotonNetwork.CurrentRoom.PlayerCount;

        if (deadPlayers.Count >= totalPlayers)
        {
            Debug.Log("모든 플레이어가 사망했습니다. 마을 씬으로 전환합니다.");

            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                StartCoroutine(ResetGame());
            }
        }
    }

    [PunRPC]
    private void RPC_ResetGame()
    {
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("모든 플레이어가 사망해 잠시 뒤 마을로 복귀합니다......");
        RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().DeleteRuntimeData();
        deadPlayers.Clear();
    }

    IEnumerator ResetGame()
    {
        photonView.RPC("RPC_ResetGame", RpcTarget.All);

        yield return new WaitForSeconds(3f);

        UIManager.Instance.CloseAllUI();

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("room");
    }
}
