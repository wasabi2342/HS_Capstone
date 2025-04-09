using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

public class RewardManager : MonoBehaviourPun
{
    public static RewardManager Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // RewardCanvas (모든 플레이어가 볼 패널)
    public TMP_Text rewardNameText;     // DetailBox 보상 이름 (로컬)
    public TMP_Text detailText;         // DetailBox 설명 (로컬)
    public RewardButton[] rewardButtons; // 버튼들 (A/B…)

    [Header("Reward Data")]
    public RewardData[] rewardDatas;

    // 마스터가 관리하는 투표: <playerId, 보상인덱스>
    private Dictionary<int, int> votes = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // UI 초기 상태
        if (rewardUI != null)
            rewardUI.SetActive(false);
        foreach (var btn in rewardButtons)
        {
            btn.Init();
        }
        if (rewardNameText != null) rewardNameText.text = "(보상 이름)";
        if (detailText != null) detailText.text = "(보상 설명)";
    }

    // ─────────────────────────────────────────────────
    // door 상호작용 등에서 호출 → 모든 클라이언트에 UI 표시
    // ─────────────────────────────────────────────────
    public void OpenRewardUI()
    {
        // UI 열기 (로컬)
        if (rewardUI != null)
            rewardUI.SetActive(true);

        // 다른 클라이언트들도 열도록 RPC
        photonView.RPC(nameof(RPC_OpenUI), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void RPC_OpenUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // ─────────────────────────────────────────────────
    // 버튼 첫 클릭 시 → 그냥 로컬 하이라이트
    // 버튼 두 번째 클릭(투표 확정) 시 → 마스터에게 RPC_ConfirmVote
    // ─────────────────────────────────────────────────
    public void RequestVote(int rewardIndex)
    {
        // 내 플레이어 ID
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        // 마스터에게 “이 플레이어가 rewardIndex 투표함”을 알림
        photonView.RPC(nameof(RPC_ConfirmVote), RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // 마스터만 실행. votes에 기록 후, 모든 클라이언트에 체크 표시
    [PunRPC]
    void RPC_ConfirmVote(int actorNum, int rewardIndex)
    {
        // 이미 투표했는지 검사
        if (votes.ContainsKey(actorNum))
        {
            Debug.Log($"[RPC_ConfirmVote] player {actorNum} already voted. ignoring.");
            return;
        }

        // 새 투표 기록
        votes[actorNum] = rewardIndex;

        // 모든 클라이언트가 체크 표시를 볼 수 있도록 RPC
        photonView.RPC(nameof(RPC_AddCheckMark), RpcTarget.All, rewardIndex);
    }

    // 모든 클라이언트에서 실행. rewardIndex 버튼에 체크 표시(2개든 1개든)
    [PunRPC]
    void RPC_AddCheckMark(int rewardIndex)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                // 원하는 만큼 체크 아이콘 생성
                btn.AddCheckIcon(); // 1
                btn.AddCheckIcon(); // 2
                break;
            }
        }
    }

    // ─────────────────────────────────────────────────
    // 보상 설명 / 상세는 로컬 전용
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
    // 투표 취소(누군가 Cancel 눌렀을 때) → 마스터에게 알림
    // ─────────────────────────────────────────────────

    public void UnhighlightAllNonVoted(RewardButton exceptButton)
    {
        // rewardButtons 배열에 담긴 모든 버튼을 순회
        foreach (var btn in rewardButtons)
        {
            // 아직 투표가 확정되지 않은(!btn.isVoted) 버튼 중,
            // 지금 클릭된 버튼(exceptButton)이 아닌 것의 노랑 하이라이트를 꺼준다
            if (!btn.isVoted && btn != exceptButton)
            {
                btn.DisableNormalHighlight();
            }
        }
    }

    public void RequestCancel()
    {
        photonView.RPC(nameof(RPC_CancelAllVotes), RpcTarget.MasterClient);
    }

    // 마스터가 votes 지우고 → 모두에게 UI 초기화 알림
    [PunRPC]
    void RPC_CancelAllVotes()
    {
        votes.Clear();
        photonView.RPC(nameof(RPC_ResetUI), RpcTarget.All);
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
