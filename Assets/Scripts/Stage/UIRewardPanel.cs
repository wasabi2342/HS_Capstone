using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance;

    [Header("UI References")]
    public GameObject rewardUI;         // 보상 UI 패널 (Canvas의 자식 패널)
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

        foreach (var btn in rewardButtons)
            btn.Init();

        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";
    }

    // 보상 UI 로컬 열기 (RPC Open UI는 PhotonNetworkManager에서 처리)
    public void OpenRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(true);
    }

    // 로컬: 투표 요청 → 중앙 PhotonNetworkManager의 RPC 호출
    public void RequestVote(int rewardIndex)
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_ConfirmVote", RpcTarget.MasterClient, actorNum, rewardIndex);
    }

    // 로컬: Cancel 버튼 동작 → 중앙 PhotonNetworkManager에 내 투표 취소 요청 RPC 호출
    public void RequestCancel()
    {
        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetworkManager.Instance.photonView.RPC("RPC_RemoveMyVote", RpcTarget.MasterClient, actorNum);
    }

    // [All Clients] PhotonNetworkManager의 RPC_AddCheckMark에 의해 호출됨
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

    // [All Clients] PhotonNetworkManager의 RPC_RemoveMyCheckIcon에 의해 호출됨
    // 로컬 플레이어의 경우, 자신의 체크 아이콘 제거와 함께 하이라이트, 버튼 상태 초기화 수행
    public void RemoveMyCheckIcons(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
                btn.RemoveCheckIcons(actorNum);
                // 로컬 플레이어라면: 해당 버튼 상태 초기화
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

                    // 다른 버튼들도 비투표 상태라면 활성화 시키기 (원하는 로직에 맞게 조정)
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

    // 로컬: 아직 투표되지 않은 버튼들의 하이라이트 제거 및 인터랙션 복원
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

    // 전체 초기화(전체 취소) 로직은 여기서는 사용하지 않습니다.
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
