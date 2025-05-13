using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private bool isInPvPArea = false;

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
            int tutorial = PlayerPrefs.GetInt("Tutorial", 0);
            if (tutorial == 0)
                PhotonNetwork.LoadLevel("Tutorial"); // 튜토리얼로
            else
                PhotonNetwork.LoadLevel("room");
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("room");
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
            PhotonNetwork.CurrentRoom.IsVisible = false;
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

    /// 최종 보상 확정 카운트다운 코루틴: 5초부터 0까지 카운트 후 최종 확정 RPC 실행
    private IEnumerator StartFinalRewardCountdown()
    {
        int countdown = 5;
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
        int countdown = 5;
        while (countdown > 0)
        {
            photonView.RPC(nameof(RPC_ShowNextStageCountdown), RpcTarget.All, countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        photonView.RPC(nameof(RPC_ShowNextStageCountdown), RpcTarget.All, 0);

        // 플레이어 오브젝트 정리
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

        // ───────── 다음 씬 이름 계산 ─────────
        string cur = SceneManager.GetActiveScene().name;      // ex) "Level0"
        string prefix = new string(cur.TakeWhile(char.IsLetter).ToArray()); // "Level"
        string numberTxt = new string(cur.SkipWhile(char.IsLetter).ToArray()); // "0"

        int curIdx;
        if (int.TryParse(numberTxt, out curIdx))
        {
            string nextScene = $"{prefix}{curIdx + 1}";

            // 빌드 세팅에 있는지 확인하고 이동
            if (Application.CanStreamedLevelBeLoaded(nextScene))
            {
                PhotonNetwork.LoadLevel(nextScene);
            }
            else
            {
                Debug.LogError($"{nextScene} 이(가) Build Settings에 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"현재 씬 이름 {cur} 에서 숫자를 찾을 수 없습니다.");
        }

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

                if (isInPvPArea)
                {
                    CheckRemainingTeamsAndEndGame();
                }
                else
                {
                    CheckAllPlayersDead();
                }
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

    private void CheckRemainingTeamsAndEndGame()
    {
        var alivePlayers = PhotonNetwork.CurrentRoom.Players
            .Where(p => !deadPlayers.Contains(p.Key))
            .Select(p => p.Value)
            .ToList();

        HashSet<int> aliveTeamIds = new HashSet<int>();

        foreach (var player in alivePlayers)
        {
            if (player.CustomProperties.TryGetValue("TeamId", out object teamIdObj) && teamIdObj is int teamId)
            {
                aliveTeamIds.Add(teamId);
            }
        }

        if (aliveTeamIds.Count == 1)
        {
            // 단 하나의 팀만 생존 → 해당 팀 승리
            int winningTeamId = aliveTeamIds.First();
            var winningPlayers = alivePlayers
                .Where(p => (int)p.CustomProperties["TeamId"] == winningTeamId)
                .ToList();

            string winnerNames = string.Join("\n", winningPlayers.Select(p => $"Player : {p.NickName}"));

            string resultMessage = winningTeamId == -1
                ? $"협력자팀 승리! \n{winnerNames}"
                : $"배신 성공: {winnerNames}";

            StartCoroutine(ResetGame(resultMessage));
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
                StartCoroutine(ResetGame("모든 플레이어가 사망해 잠시 뒤 마을로 복귀합니다......"));
            }
        }
    }

    [PunRPC]
    private void RPC_ResetGame(string message)
    {
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText(message);
        RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().DeleteRuntimeData();
        deadPlayers.Clear();
    }

    [PunRPC]
    private void RPC_GotoPVPArea(string message)
    {
        RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData();
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText(message);
    }

    public void GotoPVPArea()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LoadPVPScene());
    }

    IEnumerator LoadPVPScene()
    {
        photonView.RPC("RPC_GotoPVPArea", RpcTarget.All, "배신자가 있어 잠시 뒤 PVP지역으로 이동합니다.");

        yield return new WaitForSeconds(3f);

        UIManager.Instance.CloseAllUI();

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("PvP");
    }

    IEnumerator ResetGame(string message)
    {
        photonView.RPC("RPC_ResetGame", RpcTarget.All, message);

        // 커스텀 프로퍼티 초기화 (TeamId 제거)
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.CustomProperties.ContainsKey("TeamId"))
            {
                ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable
            {
                { "TeamId", null } // 키 제거는 null 할당 또는 Remove
            };
                player.SetCustomProperties(resetProps);
            }
        }
        yield return new WaitForSeconds(3f);

        UIManager.Instance.CloseAllUI();

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("room");
    }

    public void SetIsInPvPArea(bool value)
    {
        isInPvPArea = value;
    }

    public void EndGameInSoloPlay()
    {
        StartCoroutine(ResetGame("알파버전 클리어하셨습니다."));
    }
}
