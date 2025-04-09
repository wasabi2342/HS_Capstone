using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // ���� UI �г� (Canvas�� �ڽ� �г�)
    public TMP_Text rewardNameText;     // ���� �̸� �ؽ�Ʈ (����)
    public TMP_Text detailText;         // ���� ���� �ؽ�Ʈ (����)
    public RewardButton[] rewardButtons;// ���� ��ư �迭

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // ���� ������ �迭

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null)
            rewardNameText.text = "(���� �̸�)";
        if (detailText != null)
            detailText.text = "(���� ����)";
    }

    // ���� UI ���� ���� (RPC Open UI�� PhotonNetworkManager���� ó��)
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ����: ��ǥ ��û �� �߾� PhotonNetworkManager�� RPC ȣ��
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // ����: Cancel ��ư ���� �� �߾� PhotonNetworkManager�� �� ��ǥ ��� ��û RPC ȣ��
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // [All Clients] PhotonNetworkManager�� RPC_AddCheckMark�� ���� ȣ���
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

    // [All Clients] PhotonNetworkManager�� RPC_RemoveMyCheckIcon�� ���� ȣ���
    // ���� �÷��̾��� ���, �ڽ��� üũ ������ ���ſ� �Բ� ���̶���Ʈ, ��ư ���� �ʱ�ȭ ����
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);
                // ���� �÷��̾���: �ش� ��ư ���� �ʱ�ȭ
                if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    btn.isVoted = false;
                    btn.isSelected = false;
                    if (btn.normalHighlight != null)
                        btn.normalHighlight.enabled = false;
                    if (btn.selectHighlight != null)
                        btn.selectHighlight.enabled = false;
                    if (btn.selectRoad != null)
                        btn.selectRoad.enabled = false;
                    if (btn.unityButton != null)
                        btn.unityButton.interactable = true;

                    // �ٸ� ��ư�鵵 ����ǥ ���¶�� Ȱ��ȭ ��Ű�� (���ϴ� ������ �°� ����)
                    foreach (var otherBtn in rewardButtons)
                    {
                        if (!otherBtn.isVoted && otherBtn.unityButton != null)
                            otherBtn.unityButton.interactable = true;
                    }
                }
                break;
            }
        }
    }

    // ����: ���� ������ ǥ��
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ����: ���� ��ǥ���� ���� ��ư���� ���̶���Ʈ ���� �� ���ͷ��� ����
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

    // ��ü �ʱ�ȭ(��ü ���) ������ ���⼭�� ������� �ʽ��ϴ�.
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
