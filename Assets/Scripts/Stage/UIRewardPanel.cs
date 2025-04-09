using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class RewardManager : MonoBehaviourPun
{
    public static RewardManager Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // RewardCanvas (��� �÷��̾ �� �г�)
    public TMP_Text rewardNameText;     // DetailBox ���� �̸� (����)
    public TMP_Text detailText;         // DetailBox ���� (����)
    public RewardButton[] rewardButtons; // ��ư�� (A/B��)

    [Header("Reward Data")]
    public RewardData[] rewardDatas;

    // �����Ͱ� �����ϴ� ��ǥ: <playerId, �����ε���>
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // UI �ʱ� ����
        if (rewardUI != null)
            rewardUI.SetActive(false);
        foreach (var btn in rewardButtons)
        {
            btn.Init();
        }
        if (rewardNameText != null) rewardNameText.text = "(���� �̸�)";
        if (detailText != null) detailText.text = "(���� ����)";
    }

    // ��������������������������������������������������������������������������������������������������
    // door ��ȣ�ۿ� ��� ȣ�� �� ��� Ŭ���̾�Ʈ�� UI ǥ��
    // ��������������������������������������������������������������������������������������������������
    public void OpenRewardUI()
    {
        // UI ���� (����)
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // �ٸ� Ŭ���̾�Ʈ�鵵 ������ RPC
        photonView.RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ��������������������������������������������������������������������������������������������������
    // ��ư ù Ŭ�� �� �� �׳� ���� ���̶���Ʈ
    // ��ư �� ��° Ŭ��(��ǥ Ȯ��) �� �� �����Ϳ��� RPC_ConfirmVote
    // ��������������������������������������������������������������������������������������������������
    public void RequestVote(int rewardIndex)
    {
        // �� �÷��̾� ID
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // �����Ϳ��� ���� �÷��̾ rewardIndex ��ǥ�ԡ��� �˸�
        photonView.RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // �����͸� ����. votes�� ��� ��, ��� Ŭ���̾�Ʈ�� üũ ǥ��
    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        // �̹� ��ǥ�ߴ��� �˻�
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }

        // �� ��ǥ ���
        votes[actorNum] = rewardIndex;

        // ��� Ŭ���̾�Ʈ�� üũ ǥ�ø� �� �� �ֵ��� RPC
        photonView.RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    // ��� Ŭ���̾�Ʈ���� ����. rewardIndex ��ư�� üũ ǥ��(2���� 1����)
    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // ���ϴ� ��ŭ üũ ������ ����
                btn.AddCheckIcon(); // 1
                btn.AddCheckIcon(); // 2
                break;
            }
        }
    }

    // ��������������������������������������������������������������������������������������������������
    // ���� ���� / �󼼴� ���� ����
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
    // ��ǥ ���(������ Cancel ������ ��) �� �����Ϳ��� �˸�
    // ��������������������������������������������������������������������������������������������������

    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        // rewardButtons �迭�� ��� ��� ��ư�� ��ȸ
        foreach (var btn in rewardButtons)
        {
            // ���� ��ǥ�� Ȯ������ ����(!btn.isVoted) ��ư ��,
            // ���� Ŭ���� ��ư(exceptButton)�� �ƴ� ���� ��� ���̶���Ʈ�� ���ش�
            if (!btn.isVoted && btn != exceptButton)
            {
                btn.DisableNormalHighlight();
            }
        }
    }

    public void RequestCancel()
    {
        photonView.RPC(nameof(RPC_CancelAllVotes), RpcTarget.MasterClient);
    }

    // �����Ͱ� votes ����� �� ��ο��� UI �ʱ�ȭ �˸�
    [PunRPC]
    void RPC_CancelAllVotes()
    {
        votes.Clear();
        photonView.RPC(nameof(RPC_ResetUI), RpcTarget.All);
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
