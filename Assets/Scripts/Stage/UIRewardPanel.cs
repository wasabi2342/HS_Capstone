using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // 보상 UI 패널 (Canvas의 자식)
    public TMP_Text rewardNameText;     // 보상 이름 텍스트 (로컬)
    public TMP_Text detailText;         // 보상 설명 텍스트 (로컬)
    public RewardButton[] rewardButtons; // 보상 버튼 배열

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // 보상 데이터

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

    // 열기: 로컬에서 보상 UI 활성화하고, 다른 클라이언트는 중앙 RPC(PhotonNetworkManager)를 통해 처리됨
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
        PhotonNetworkManager.Instance.photonView.RPC("RPC_OpenUI", RpcTarget.OthersBuffered);
    }

    // 투표 요청: 로컬 버튼에서 호출 → 중앙 RPC 호출
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // 취소 요청: 로컬 버튼에서 호출 → 중앙 RPC로 본인 투표만 제거
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // 중앙 RPC (RPC_AddCheckMark) 호출 시 실행: 각 보상 버튼에 대해 체크 아이콘 추가
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

    // 중앙 RPC (RPC_RemoveMyCheckIcon) 호출 시 실행: 해당 플레이어의 체크 아이콘 제거
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

    // 로컬: 아직 투표 확정되지 않은 버튼들 중, 선택되지 않은 버튼의 하이라이트 제거 및 인터랙션 복원
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

    // Reset UI: 전체 초기화 (전체 취소는 사용하지 않고 개별 취소만 함)
    public void ResetUI()
    {
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }
}
