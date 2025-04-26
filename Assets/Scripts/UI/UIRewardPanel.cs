using Photon.Pun;
using TMPro;
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
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

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

    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);
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

    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

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

    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }
    public override void OnDisable()
    {
        base.OnDisable(); 

        // 보상 UI가 사라졌으니 입력을 Player 맵으로 복귀
        if (InputManager.Instance != null)
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }
}
