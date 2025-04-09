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
    public TMP_Text rewardNameText;     // 보상 이름 (로컬)
    public TMP_Text detailText;         // 보상 설명 (로컬)
    public RewardButton[] rewardButtons; // 보상 선택 버튼들

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // 보상 데이터 배열

    // 마스터가 관리하는 투표 기록: <플레이어ID, 보상 인덱스>
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 초기 상태: 보상 UI 숨김
        if (rewardUI != null)
            rewardUI.SetActive(false);

        // 각 보상 버튼 초기화
        foreach (var btn in rewardButtons)
        {
            btn.Init();
        }
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    // 모든 클라이언트에 보상 UI를 열도록 하는 RPC 호출 메서드
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);

        PhotonView.Get(this).RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // 투표 요청 (RPC 통해 마스터로 전송)
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonView.Get(this).RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }
        votes[actorNum] = rewardIndex;
        PhotonView.Get(this).RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
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

    // 보상 상세정보 로컬에서 표시
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // 아직 투표 확정되지 않은 버튼들 중, 선택된 버튼를 제외하고 하이라이트를 제거하고 버튼 기능을 활성화 (로컬)
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

    // 투표 취소 요청 (마스터에게 RPC 전송 -> 모든 클라이언트에 초기 상태 복원)
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
        // 모든 버튼 초기화 (체크 아이콘 제거 등)
        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null) rewardNameText.text = "(보상 이름)";
        if (detailText != null) detailText.text = "(보상 설명)";
    }
}
