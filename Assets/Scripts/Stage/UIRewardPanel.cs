using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // ���� UI �г� (Canvas�� �ڽ� �г�)
    public TMP_Text rewardNameText;     // ���� �̸� �ؽ�Ʈ (����)
    public TMP_Text detailText;         // ���� ���� �ؽ�Ʈ (����)
    public RewardButton[] rewardButtons; // ���� ���� ��ư��

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // ���� ������ �迭

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (rewardUI != null)
            rewardUI.SetActive(false);

        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(���� �̸�)";
        if (detailText != null)
            detailText.text = "(���� ����)";
    }

    // ���ÿ��� ���� UI ����; �ٸ� Ŭ���̾�Ʈ�� PhotonNetworkManager�� ���� RPC�� ����
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
        PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // ��ǥ ��û: ���� ��ư���� ȣ�� �� �߾� �Ŵ����� PhotonView�� ���� RPC ����
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // ��ǥ ��� ��û: ���� ��ư(ĵ��)���� ȣ��
    public void RequestCancel()
    {
        PhotonNetworkManager.Instance.photonView.RPC("RPC_CancelAllVotes", RpcTarget.MasterClient);
    }

    // ���� ������Ʈ �Լ�: üũ ������ �߰� (��ǥ Ȯ�� ��)
    public void AddCheckMark(int rewardIndex)
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

    // ���� UI �ʱ�ȭ (��ǥ ��� ��)
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(���� �̸�)";
        if (detailText != null)
            detailText.text = "(���� ����)";
    }

    // ���ÿ��� ���� ������ ǥ��
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ����: ���� ��ǥ Ȯ������ ���� ��ư�� ��, ���� ���õ� ��ư �̿��� ��ư ���̶���Ʈ ���� �� ���ͷ��� ����
    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        foreach (var btn in rewardButtons)
        {
            if (!btn.isVoted && btn != exceptButton)
            {
                btn.DisableNormalHighlight();
                if (btn.unityButton != null)
                    btn.unityButton.interactable = true;
            }
        }
    }
}
