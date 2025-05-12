using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIRewardPanel : UIBase
{
    public static UIRewardPanel Instance { get; private set; }

    [Header("UI References")]
    public GameObject rewardUI;

    /* ───── 기존 필드 ───── */
    public RewardData[] rewardDatas;
    public RewardButton[] rewardButtons;
    public TMPro.TMP_Text rewardNameText, detailText;

    /* ───────── [ADD] 진행 바 & 상세창용 필드 ───────── */
    [Header("Stage Progress Bar")]
    [SerializeField] private Image stageFillImage;   // 왼쪽 위 분홍색 Bar(Image의 Fill)
    [SerializeField] private int maxStageCount = 5;   // Level0~4 ⇒ 5

    [Header("Detail Box")]
    [SerializeField] private GameObject detailRoot;     // 오른쪽 전체 패널
    [SerializeField] private Image detailIcon;     // 아이콘
    /* 1) 자료구조 : 아이콘 오브젝트 외에 "투표 순서" 리스트 추가 */
    private readonly Dictionary<int, GameObject> playerIconMap = new();
    private readonly List<int> voteOrder = new();

    private void Awake()
    {
        Instance = this;
        InitStageBar();          // ← ADD
        if (detailRoot) detailRoot.SetActive(false);
        if (detailIcon) detailIcon.gameObject.SetActive(false);
        if (rewardNameText) rewardNameText.gameObject.SetActive(false);
        if (detailText) detailText.gameObject.SetActive(false);
    }
    /* ───── [ADD] 스테이지 진행 바 채우기 ───── */
    void InitStageBar()
    {
        if (!stageFillImage) return;

        string s = SceneManager.GetActiveScene().name; // "Level2"
        int idx = 0;
        if (s.StartsWith("Level") &&
            int.TryParse(s.Substring(5), out var n)) idx = n;

        stageFillImage.fillAmount = Mathf.Clamp01((idx + 1f) / (float)maxStageCount);
    }
    /* ───── RewardButton에서 호출되는 기존/신규 상세 표시 ───── */
    public void ShowDetailLocal(int rewardIdx)         // ★ RewardButton과 이름 맞춤
    {
        if (rewardIdx < 0 || rewardIdx >= rewardDatas.Length) return;

        var data = rewardDatas[rewardIdx];
        if (detailRoot && !detailRoot.activeSelf) detailRoot.SetActive(true);
        if (detailIcon && !detailIcon.gameObject.activeSelf)
            detailIcon.gameObject.SetActive(true);
        if (rewardNameText && !rewardNameText.gameObject.activeSelf)
            rewardNameText.gameObject.SetActive(true);
        if (detailText && !detailText.gameObject.activeSelf)
            detailText.gameObject.SetActive(true);

        if (detailIcon) detailIcon.sprite = data.Icon;
        if (rewardNameText) rewardNameText.text = data.rewardName;
        if (detailText) detailText.text = data.rewardDetail;
    }
    private void Start()
    {
        // 플레이어 인풋 안받기
        if (InputManager.Instance != null)
            InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        foreach (var btn in rewardButtons)
            btn.Init();
        if (rewardNameText != null)
            rewardNameText.text = "(보상 이름)";
        if (detailText != null)
            detailText.text = "(보상 설명)";

        // 게임데이터 저장 하기
        RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().SaveRunTimeData(); 
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
        if (detailRoot) detailRoot.SetActive(false);
        if (detailIcon) detailIcon.gameObject.SetActive(false);
        if (rewardNameText) rewardNameText.gameObject.SetActive(false);
        if (detailText) detailText.gameObject.SetActive(false);
    }

    public void AddCheckMark(int rewardIndex, int actorNum)
    {
        foreach (var btn in rewardButtons)
        {
            if (btn.rewardIndex == rewardIndex)
            {
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
                btn.RemoveCheckIcon(actorNum);
                if (actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    btn.isVoted = false;
                    btn.isSelected = false;
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

    public void UnselectAllNonVoted(RewardButton exceptButton)
    {
        foreach (var btn in rewardButtons)
        {
            if (!btn.isVoted && btn != exceptButton)
            {
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
