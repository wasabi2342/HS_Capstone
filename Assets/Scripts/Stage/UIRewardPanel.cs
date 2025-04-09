using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // 보상 UI 패널 (Canvas의 자식 패널)
    public TMP_Text rewardNameText;     // 보상 이름 텍스트 (로컬)
    public TMP_Text detailText;         // 보상 설명 텍스트 (로컬)
    public RewardButton[] rewardButtons; // 보상 선택 버튼들

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // 보상 데이터 배열

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
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    // 로컬에서 보상 UI 열기; 다른 클라이언트는 PhotonNetworkManager를 통해 RPC로 열림
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
        PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // 투표 요청: 로컬 버튼에서 호출 → 중앙 매니저의 PhotonView를 통해 RPC 전송
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // 투표 취소 요청: 로컬 버튼(캔슬)에서 호출
    public void RequestCancel()
    {
        PhotonNetworkManager.Instance.photonView.RPC("RPC_CancelAllVotes", RpcTarget.MasterClient);
    }

    // 로컬 업데이트 함수: 체크 아이콘 추가 (투표 확정 시)
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

    // 로컬 UI 초기화 (투표 취소 시)
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    // 로컬에서 보상 상세정보 표시
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // 로컬: 아직 투표 확정되지 않은 버튼들 중, 현재 선택된 버튼 이외의 버튼 하이라이트 제거 및 인터랙션 복원
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
