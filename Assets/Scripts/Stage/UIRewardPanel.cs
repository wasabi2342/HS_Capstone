using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;
    public TMP_Text rewardNameText;
    public TMP_Text detailText;
    public RewardButton[] rewardButtons;

    [Header("Reward Data")]
    public RewardData[] rewardDatas;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null) rewardNameText.text = "(���� �̸�)";
        if (detailText != null) detailText.text = "(���� ����)";
    }

    // ���� UI ���� (�� ��ȣ�ۿ뿡�� ��Ʈ��ũ RPC�� ���� ���� �ְ�, ���ÿ��� �� ���� ����)
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // �ٸ� Ŭ���̾�Ʈ�� ������ �ϰ� �ʹٸ�: 
        // PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // ��ư �� ��° Ŭ�� �� RequestVote���� ȣ��
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // Cancel ��ư �� �� ��ǥ�� ���
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // �����Ϳ��� ������ ��ǥ ������ ��û
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // [All Clients] RPC_AddCheckMark�� ȣ��Ǹ� AddCheckMark(rewardIndex, actorNum) ����
    public void AddCheckMark(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // �� �� ȣ���ϸ� �� �� ������
                btn.AddCheckIcon(actorNum);
                btn.AddCheckIcon(actorNum);
                break;
            }
        }
    }

    // [All Clients] RPC_RemoveMyCheckIcon�� ȣ��Ǹ� RemoveMyCheckIcons(rewardIndex, actorNum) ����
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);

                // ������������������������������������������������������������������������������������
                // [�߰�] "����" ����ߴٸ� -> �� ��ư �ٽ� Ȱ��ȭ
                // ������������������������������������������������������������������������������������
                if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // �� ��ư�� ���� isVoted/isSelected ����
                    btn.isVoted = false;
                    btn.isSelected = false;
                    // �� ��ư�� �ٽ� ���� �� �ֵ��� Ȱ��ȭ
                    if (btn.unityButton != null)
                        btn.unityButton.interactable = true;

                    // Ȥ�� �ٸ� ��ư�鵵 ��Ȱ��ȭ�� ���¶�� �ٽ� Ȱ��ȭ
                    // (���ϴ� ������ �°� ���� ����)
                    foreach (var otherBtn in rewardButtons)
                    {
                        // ���� ��ǥ�� Ȯ������ �ʾҴٸ� ��Ȱ��ȭ
                        if (!otherBtn.isVoted)
                            otherBtn.unityButton.interactable = true;
                    }
                }
                break;
            }
        }
    }

    // ���� ������ ���� ǥ��
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ù Ŭ�� �� ��� ���̶���Ʈ�� �����, �ٸ� ��ư ���̶���Ʈ �� ���ͷ����� ����
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

    // ��ü �ʱ�ȭ(��� ��ǥ ����) ������ �����ϰų� �ʿ� �� ���� ����
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null) rewardNameText.text = "(���� �̸�)";
        if (detailText != null) detailText.text = "(���� ����)";
    }
}
