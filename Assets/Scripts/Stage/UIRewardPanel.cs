using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // 보상 UI 패널 (Canvas의 자식)
    public TMP_Text rewardNameText;     // 보상 이름 텍스트 (로컬)
    public TMP_Text detailText;         // 보상 설명 텍스트 (로컬)
    public RewardButton[] rewardButtons;// 보상 버튼 배열

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

    // 로컬에서 보상 UI 활성화 (문 상호작용 등에서 호출)
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // 투표 요청: 로컬 버튼에서 호출 > 중앙 PhotonNetworkManager를 통해 RPC 전송
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // Cancel 버튼 호출: 로컬 플레이어의 투표만 취소
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // [All Clients] 중앙에서 RPC 호출 시, 해당 보상 버튼에 체크 아이콘 추가
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

    // [All Clients] 중앙에서 RPC 호출 시, 해당 보상 버튼에서 로컬 플레이어의 체크 아이콘 제거
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);
                if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // 내 버튼 상태 초기화: 하이라이트 및 버튼 활성화
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
                    // 다른 버튼들도 재활성화 (투표 확정되지 않은 버튼)
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

    // 로컬: 보상 상세정보 표시
    public void ShowDetailLocal(int rewardIndex)
    {
        if (rewardDatas == null || rewardIndex < 0 || rewardIndex >= rewardDatas.Length)
            return;
        if (rewardNameText != null)
            rewardNameText.text = rewardDatas[rewardIndex].rewardName;
        if (detailText != null)
            detailText.text = rewardDatas[rewardIndex].rewardDetail;
    }

    // 로컬: 아직 투표 확정되지 않은 버튼들에 대해 하이라이트 제거 및 인터랙션 복원
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

    // 전체 초기화 함수 (전체 취소 시 구현 가능 - 현재는 개별 취소만 사용)
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
