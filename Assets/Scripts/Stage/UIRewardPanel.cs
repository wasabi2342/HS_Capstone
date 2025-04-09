using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // ���� UI �г� (Canvas�� �ڽ�)
    public TMP_Text rewardNameText;     // ���� �̸� �ؽ�Ʈ (����)
    public TMP_Text detailText;         // ���� ���� �ؽ�Ʈ (����)
    public RewardButton[] rewardButtons; // ���� ��ư �迭

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // ���� ������

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

    // ����: ���ÿ��� ���� UI Ȱ��ȭ�ϰ�, �ٸ� Ŭ���̾�Ʈ�� �߾� RPC(PhotonNetworkManager)�� ���� ó����
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
        PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // ��ǥ ��û: ���� ��ư���� ȣ�� �� �߾� RPC ȣ��
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // ��� ��û: ���� ��ư���� ȣ�� �� �߾� RPC�� ���� ��ǥ�� ����
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // �߾� RPC (RPC_AddCheckMark) ȣ�� �� ����: �� ���� ��ư�� ���� üũ ������ �߰�
    public void AddCheckMark(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.AddCheckIcon(actorNum);
                btn.AddCheckIcon(actorNum);
                break;
            }
        }
    }

    // �߾� RPC (RPC_RemoveMyCheckIcon) ȣ�� �� ����: �ش� �÷��̾��� üũ ������ ����
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);
                break;
            }
        }
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

    // ����: ���� ��ǥ Ȯ������ ���� ��ư�� ��, ���õ��� ���� ��ư�� ���̶���Ʈ ���� �� ���ͷ��� ����
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

    // Reset UI: ��ü �ʱ�ȭ (��ü ��Ҵ� ������� �ʰ� ���� ��Ҹ� ��)
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(���� �̸�)";
        if (detailText != null)
            detailText.text = "(���� ����)";
    }
}
