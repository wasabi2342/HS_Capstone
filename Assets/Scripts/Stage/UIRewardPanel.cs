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

        if (rewardNameText != null) rewardNameText.text = "(보상 이름)";
        if (detailText != null) detailText.text = "(보상 설명)";
    }

    // 보상 UI 오픈 (문 상호작용에서 네트워크 RPC로 열릴 수도 있고, 로컬에서 열 수도 있음)
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // 다른 클라이언트도 열도록 하고 싶다면: 
        // PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // 버튼 두 번째 클릭 시 RequestVote에서 호출
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // Cancel 버튼 → 내 투표만 취소
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // 마스터에게 “나의 투표 삭제” 요청
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // [All Clients] RPC_AddCheckMark가 호출되면 AddCheckMark(rewardIndex, actorNum) 실행
    public void AddCheckMark(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // 두 번 호출하면 두 개 아이콘
                btn.AddCheckIcon(actorNum);
                btn.AddCheckIcon(actorNum);
                break;
            }
        }
    }

    // [All Clients] RPC_RemoveMyCheckIcon이 호출되면 RemoveMyCheckIcons(rewardIndex, actorNum) 실행
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);

                // ──────────────────────────────────────────
                // [추가] "내가" 취소했다면 -> 내 버튼 다시 활성화
                // ──────────────────────────────────────────
                if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // 이 버튼에 대한 isVoted/isSelected 해제
                    btn.isVoted = false;
                    btn.isSelected = false;
                    // 내 버튼을 다시 누를 수 있도록 활성화
                    if (btn.unityButton != null)
                        btn.unityButton.interactable = true;

                    // 혹시 다른 버튼들도 비활성화된 상태라면 다시 활성화
                    // (원하는 로직에 맞게 조절 가능)
                    foreach (var otherBtn in rewardButtons)
                    {
                        // 아직 투표가 확정되지 않았다면 재활성화
                        if (!otherBtn.isVoted)
                            otherBtn.unityButton.interactable = true;
                    }
                }
                break;
            }
        }
    }

    // 보상 상세정보 로컬 표시
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // 첫 클릭 시 노란 하이라이트만 남기고, 다른 버튼 하이라이트 및 인터랙션을 복원
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

    // 전체 초기화(모든 투표 제거) 로직은 생략하거나 필요 시 구현 가능
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null) rewardNameText.text = "(보상 이름)";
        if (detailText != null) detailText.text = "(보상 설명)";
    }
}
