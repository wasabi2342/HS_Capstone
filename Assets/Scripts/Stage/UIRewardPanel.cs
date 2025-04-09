using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // ��� �÷��̾ �� ���� �г�(RewardCanvas)
    public TMP_Text rewardNameText;     // ���� �̸��� ǥ���� �ؽ�Ʈ (����)
    public TMP_Text detailText;         // ���� ������ ǥ���� �ؽ�Ʈ (����)
    public RewardButton[] rewardButtons; // ���� ���� ��ư��

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // ���� ������ �迭

    // �����Ͱ� �����ϴ� ��ǥ ���: <�÷��̾�ID, ���� �ε���>
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // ���� ��ư �ʱ�ȭ
        foreach (var btn in rewardButtons)
        {
            btn.Init();
        }
        if (rewardNameText != null)
            rewardNameText.text = "(���� �̸�)";
        if (detailText != null)
            detailText.text = "(���� ����)";
    }

    // ����������������������������������������������������������
    // door �Ǵ� �ٸ� ��ȣ�ۿ뿡�� ȣ�� �� ��� Ŭ���̾�Ʈ�� ���� UI ǥ��
    // ����������������������������������������������������������
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);

        PhotonView.Get(this).RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ����������������������������������������������������������
    // ���� ��ư ù Ŭ�� �� ���� ������ ǥ��, �� ��° Ŭ�� �� ��ǥ ��û
    // ����������������������������������������������������������
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView.Get(this).RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }
        votes[actorNum] = rewardIndex;
        PhotonView.Get(this).RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.AddCheckIcon();
                btn.AddCheckIcon();
                break;
            }
        }
    }

    // ����������������������������������������������������������
    // ���� ������ ���ÿ��� ǥ��
    // ����������������������������������������������������������
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ����������������������������������������������������������
    // ��ǥ�� Ȯ������ ���� ��ư�� ���̶���Ʈ ����
    // ����������������������������������������������������������
    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        foreach (var btn in rewardButtons)
        {
            if (!btn.isVoted && btn != exceptButton)
            {
                btn.DisableNormalHighlight();
            }
        }
    }

    public void RequestCancel()
    {
        PhotonView.Get(this).RPC(nameof(RPC_CancelAllVotes), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_CancelAllVotes()
    {
        votes.Clear();
        PhotonView.Get(this).RPC(nameof(RPC_ResetUI), RpcTarget.All);
    }

    [PunRPC]
    void RPC_ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null) rewardNameText.text = "(���� �̸�)";
        if (detailText != null) detailText.text = "(���� ����)";
    }
}
