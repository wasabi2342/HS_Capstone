using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // RewardCanvas (모든 플레이어가 볼 패널)
    public TMP_Text rewardNameText;     // 보상 이름 텍스트 (로컬)
    public TMP_Text detailText;         // 보상 설명 텍스트 (로컬)
    public RewardButton[] rewardButtons; // 보상 선택 버튼들 (A/B 등)

    [Header("Reward Data")]
    public RewardData[] rewardDatas;    // 보상 데이터 배열

    // 마스터가 관리하는 투표 (playerId, 보상인덱스)
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 초기 상태: rewardUI는 꺼둡니다.
        if (rewardUI != null)
            rewardUI.SetActive(false);

        // 보상 버튼 초기화
        foreach (var btn in rewardButtons)
        {
            btn.Init();
        }
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    // ─────────────────────────────────────────────────
    // door 상호작용 등에서 호출 → 모든 클라이언트에 보상 UI 표시
    // ─────────────────────────────────────────────────
    public void OpenRewardUI()
    {
        // 로컬에서 보상 UI 활성화
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // 다른 클라이언트에서도 보상 UI가 열리도록 RPC 호출
        PhotonView.Get(this).RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ─────────────────────────────────────────────────
    // 버튼 첫 클릭 시 로컬 하이라이트, 두 번째 클릭(투표 확정) 시 마스터에 투표 요청
    // ─────────────────────────────────────────────────
    public void RequestVote(int rewardIndex)
    {
        // 내 플레이어 ID
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // 마스터에게 "이 플레이어가 rewardIndex에 투표"했음을 알림
        PhotonView.Get(this).RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        // 이미 투표한 플레이어인지 검사
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }

        // 새 투표 기록
        votes[actorNum] = rewardIndex;

        // 모든 클라이언트에 rewardIndex 버튼에 체크 아이콘 추가하도록 RPC 호출
        PhotonView.Get(this).RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // 원하는 만큼 체크 아이콘 추가 (예: 두 번 추가)
                btn.AddCheckIcon();
                btn.AddCheckIcon();
                break;
            }
        }
    }

    // ─────────────────────────────────────────────────
    // 보상 상세 정보를 로컬에서 표시
    // ─────────────────────────────────────────────────
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // ─────────────────────────────────────────────────
    // 투표 취소 요청: 마스터에게 알림
    // ─────────────────────────────────────────────────
    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        // rewardButtons의 모든 버튼을 순회하며 아직 투표가 확정되지 않은 버튼의 하이라이트 해제
        foreach (var btn in rewardButtons)
        {
            if (!btn.isVoted && btn != exceptButton)
            {
                btn.DisableNormalHighlight();
            }
        }
    }

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
        // 모든 버튼 초기화
        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null) rewardNameText.text = "(보상 이름)";
        if (detailText != null) detailText.text = "(보상 설명)";
    }
}
