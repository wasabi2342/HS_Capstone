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

        Debug.Log("���漭�� �κ� ����");
    }

    public void ConnectPhoton()
    {
        if (PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("���漭�� ����");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText(message);
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
        Debug.Log("60�� ��� �������� ����");
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
            Debug.Log("��� �غ� �Ϸ�");
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
        // �߾� UIManager�� ���� UIRewardPanel �������� Instantiate
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
    /// ��ǥ Ȯ�� RPC: �̹� ��ǥ�� �÷��̾�� ����, �ƴ϶�� ��� �� üũ ������ �߰�
    [PunRPC]
    public void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        if (rewardVotes.ContainsKey(actorNum))
            return;

        rewardVotes[actorNum] = rewardIndex;
        photonView.RPC("RPC_AddCheckMark", RpcTarget.All, actorNum, rewardIndex);

        // ��� �ο��� ��ǥ �Ϸ��ϸ� ī��Ʈ�ٿ� ���� (������ Ŭ���̾�Ʈ)
        if (rewardVotes.Count == PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
        {
            finalRewardCountdownCoroutine = StartCoroutine(StartFinalRewardCountdown());
        }
    }

    /// Ư�� �÷��̾�(actorNum)�� ��ǥ�� ����(rewardIndex)�� ���� üũ ������ �߰�
    [PunRPC]
    public void RPC_AddCheckMark(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.AddCheckMark(rewardIndex, actorNum);
        }
    }

    /// ���� �÷��̾ ��� ��û �� �ش� �÷��̾��� ��ǥ�� ����
    [PunRPC]
    public void RPC_RemoveMyVote(int actorNum)
    {
        if (!rewardVotes.ContainsKey(actorNum))
            return;

        int rewardIndex = rewardVotes[actorNum];
        rewardVotes.Remove(actorNum);

        photonView.RPC("RPC_RemoveMyCheckIcon", RpcTarget.All, actorNum, rewardIndex);

        // �̹� ī��Ʈ�ٿ��� ���� ���̶�� ��� �ߴ�
        if (PhotonNetwork.IsMasterClient && finalRewardCountdownCoroutine != null)
        {
            StopCoroutine(finalRewardCountdownCoroutine);
            finalRewardCountdownCoroutine = null;
        }
    }

    /// Ư�� ���󿡼� �ش� �÷��̾�(actorNum)�� �߰��� üũ ������ ����
    [PunRPC]
    public void RPC_RemoveMyCheckIcon(int actorNum, int rewardIndex)
    {
        if (UIRewardPanel.Instance != null)
        {
            UIRewardPanel.Instance.RemoveMyCheckIcons(rewardIndex, actorNum);
        }
    }

    /// ī��Ʈ�ٿ� ������Ʈ RPC: RewardName �ؽ�Ʈ�� ���� �� ǥ��
    [PunRPC]
    public void RPC_UpdateCountdown(int seconds, string unused)
    {
        if (UIRewardPanel.Instance != null && UIRewardPanel.Instance.rewardNameText != null)
        {
            // ���� �ð��� ������ ǥ��, ������ �ؽ�Ʈ�� ����
            UIRewardPanel.Instance.rewardNameText.text = seconds > 0 ? seconds.ToString() : "";
        }
    }

    /// ���� ���� Ȯ�� ī��Ʈ�ٿ� �ڷ�ƾ: 10�ʺ��� 0���� ī��Ʈ �� ���� Ȯ�� RPC ����
    private IEnumerator StartFinalRewardCountdown()
    {
        int countdown = 10;
        while (countdown > 0)
        {
            photonView.RPC("RPC_UpdateCountdown", RpcTarget.All, countdown, "");
            yield return new WaitForSeconds(1f);
            countdown--;

            // (����) �߰��� ������ ��ǥ ��������� ���⼭�� break
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
    ///���� ���� Ȯ�� RPC: weighted random selection �� ��� ����
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

        Debug.Log($"���� ���õ� ���� �ε���: {winningIndex} (Ȯ��: {probabilities[winningIndex]:0.00}), ������: {r}");
        // ���� ����� winningIndex �ϳ��� �����մϴ�.
        photonView.RPC("RPC_UpdateFinalReward", RpcTarget.All, winningIndex);

        photonView.RPC("RPC_SaveRunTimeData", RpcTarget.All); // ���� ������ ����

        if (PhotonNetwork.IsMasterClient)
        {
            // ������ ���� ���� �ؽ����̺� ����
            var props = new ExitGames.Client.Photon.Hashtable();
            props["FinalRewardIndex"] = winningIndex;

            // ���� ���� CustomProperties�� ����
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    /// ���� ��÷ ��� ������Ʈ: RewardName �ؽ�Ʈ�� ����� Detail �ؽ�Ʈ�� ��÷ ����� ǥ��
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
        int countdown = 5; // ��) 5�� �� ���� �� �̵�
        while (countdown > 0)
        {
            // ��� Ŭ���̾�Ʈ�� 5,4,3,... �޽����� �� �� �ֵ��� RPC
            photonView.RPC("RPC_ShowNextStageCountdown", RpcTarget.All, countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // ī��Ʈ�ٿ��� ������ ���������� 0�� ǥ��
        photonView.RPC("RPC_ShowNextStageCountdown", RpcTarget.All, 0);
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
        // ���� ���ϴ� ������ �̵�
        PhotonNetwork.LoadLevel("Level1");

        finalRewardNextStageCoroutine = null;
    }
    [PunRPC]
    public void RPC_ShowNextStageCountdown(int seconds)
    {
        if (UIRewardPanel.Instance != null)
        {
            // detailText �Ʒ��� �߰��� ���ų�, ������ Text �ʵ带 ���� ���
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

        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText($"��ΰ� ������ ���ط���{damageBuff}�� �����մϴ�.");
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

        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText($"��ΰ� ����� ������ ���ط���{damageBuff}�� �����մϴ�.");
    }

    [PunRPC]
    public void PopupBlessingPanel()
    {
        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanel<UISelectBlessingPanel>();

        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText($"��ſ� ������ ��ȣ�� ȹ���մϴ�.");
    }

    [PunRPC]
    public void PopupDialogPanel(string message)
    {
        if (UIManager.Instance.ReturnPeekUI() as UICoopOrBetrayPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }

        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText(message);
    }
}
