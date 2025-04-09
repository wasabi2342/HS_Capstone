using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // RewardCanvas (��� �÷��̾ �� �г�)
    public TMP_Text rewardNameText;     // ���� �̸� �ؽ�Ʈ (����)
    public TMP_Text detailText;         // ���� ���� �ؽ�Ʈ (����)
    public RewardButton[] rewardButtons; // ���� ���� ��ư�� (A/B ��)

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // ���� ������ �迭

    // �����Ͱ� �����ϴ� ��ǥ (playerId, �����ε���)
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // �ʱ� ����: rewardUI�� ���Ӵϴ�.
        if (rewardUI != null)
            rewardUI.SetActive(false);

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

    // ��������������������������������������������������������������������������������������������������
    // door ��ȣ�ۿ� ��� ȣ�� �� ��� Ŭ���̾�Ʈ�� ���� UI ǥ��
    // ��������������������������������������������������������������������������������������������������
    public void OpenRewardUI()
    {
        // ���ÿ��� ���� UI Ȱ��ȭ
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // �ٸ� Ŭ���̾�Ʈ������ ���� UI�� �������� RPC ȣ��
        PhotonView.Get(this).RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ��������������������������������������������������������������������������������������������������
    // ��ư ù Ŭ�� �� ���� ���̶���Ʈ, �� ��° Ŭ��(��ǥ Ȯ��) �� �����Ϳ� ��ǥ ��û
    // ��������������������������������������������������������������������������������������������������
    public void RequestVote(int rewardIndex)
    {
        // �� �÷��̾� ID
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // �����Ϳ��� "�� �÷��̾ rewardIndex�� ��ǥ"������ �˸�
        PhotonView.Get(this).RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        // �̹� ��ǥ�� �÷��̾����� �˻�
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }

        // �� ��ǥ ���
        votes[actorNum] = rewardIndex;

        // ��� Ŭ���̾�Ʈ�� rewardIndex ��ư�� üũ ������ �߰��ϵ��� RPC ȣ��
        PhotonView.Get(this).RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // ���ϴ� ��ŭ üũ ������ �߰� (��: �� �� �߰�)
                btn.AddCheckIcon();
                btn.AddCheckIcon();
                break;
            }
        }
    }

    // ��������������������������������������������������������������������������������������������������
    // ���� �� ������ ���ÿ��� ǥ��
    // ��������������������������������������������������������������������������������������������������
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ��������������������������������������������������������������������������������������������������
    // ��ǥ ��� ��û: �����Ϳ��� �˸�
    // ��������������������������������������������������������������������������������������������������
    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        // rewardButtons�� ��� ��ư�� ��ȸ�ϸ� ���� ��ǥ�� Ȯ������ ���� ��ư�� ���̶���Ʈ ����
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
        // ��� ��ư �ʱ�ȭ
        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null) rewardNameText.text = "(���� �̸�)";
        if (detailText != null) detailText.text = "(���� ����)";
    }
}
