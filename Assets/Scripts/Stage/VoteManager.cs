using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Collections.Generic;

public class VoteManager : MonoBehaviourPun
{
    public static VoteManager Instance;

    // 기존에 에디터에서 가져오던 참조들을 없애고, 코드에서 만들기
    private Canvas mainCanvas;
    private RectTransform canvasRect;
    private GameObject votePanel;
    private Button voteAButton;
    private Button voteBButton;

    private bool isVoting = false;
    private Dictionary<int, string> votes = new Dictionary<int, string>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 처음에 UI를 아예 안 만든 상태. (필요 시 미리 만들어둘 수도 있음)
        // -> 문 상호작용(RPC_ShowVoteUI) 시점에서 ShowVoteUI()를 호출해 만듦

        // EventSystem이 없으면 생성
        if (!FindObjectOfType<EventSystem>())
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    // ───────────────────────────────────────────────────────────────────
    // 문에 상호작용 → RPC_ShowVoteUI → VoteManager.Instance.ShowVoteUI() 호출
    // ───────────────────────────────────────────────────────────────────
    public void ShowVoteUI()
    {
        if (!votePanel)
        {
            CreateVoteUI();
        }

        isVoting = true;
        votes.Clear();

        if (votePanel)
            votePanel.SetActive(true);
    }

    // ───────────────────────────────────────────────────────────────────
    // 코드로 Canvas & Panel & Button 만들기
    // ───────────────────────────────────────────────────────────────────
    private void CreateVoteUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("VoteCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasRect = mainCanvas.GetComponent<RectTransform>();

        canvasObj.AddComponent<GraphicRaycaster>();

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Panel
        votePanel = new GameObject("VotePanel");
        votePanel.transform.SetParent(canvasRect, false);

        RectTransform panelRect = votePanel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelBg = votePanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.5f);

        // A Button
        voteAButton = CreateButtonObject("VoteA", votePanel.transform,
            new Vector2(-80, -50), "A 보상");
        voteAButton.onClick.AddListener(() => OnClickVote("A"));

        // B Button
        voteBButton = CreateButtonObject("VoteB", votePanel.transform,
            new Vector2(80, -50), "B 보상");
        voteBButton.onClick.AddListener(() => OnClickVote("B"));

        votePanel.SetActive(false);
    }

    private Button CreateButtonObject(string objName, Transform parent, Vector2 anchoredPos, string btnText)
    {
        GameObject btnObj = new GameObject(objName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 50);
        rt.anchoredPosition = anchoredPos;

        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();

        // 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(120, 50);

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = btnText;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.black;
        textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComp.fontSize = 20;

        return btn;
    }

    // ───────────────────────────────────────────────────────────────────
    // 기존 투표 로직 (OnClickVote → RPC_SubmitVote → TallyVotes)
    // ───────────────────────────────────────────────────────────────────
    private void OnClickVote(string option)
    {
        if (!isVoting) return;

        // 플레이어가 버튼 클릭 → 마스터에게 “내 투표”를 알림
        photonView.RPC("RPC_SubmitVote", RpcTarget.MasterClient,
            PhotonNetwork.LocalPlayer.ActorNumber, option);

        // 패널 닫기
        votePanel.SetActive(false);
    }

    [PunRPC]
    void RPC_SubmitVote(int actorNumber, string option)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        votes[actorNumber] = option;
        if (votes.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            TallyVotes();
        }
    }

    void TallyVotes()
    {
        int countA = 0, countB = 0;
        foreach (var v in votes.Values)
        {
            if (v == "A") countA++;
            else if (v == "B") countB++;
        }

        string winner = (countA >= countB) ? "A" : "B";

        isVoting = false;
        photonView.RPC("RPC_AnnounceResult", RpcTarget.All, winner);
    }

    [PunRPC]
    void RPC_AnnounceResult(string winner)
    {
        Debug.Log("최종 보상은: " + winner);

        if (PhotonNetwork.IsMasterClient)
        {
            // 다음 씬 로드, 커스텀 프로퍼티 저장 등
            var cp = PhotonNetwork.CurrentRoom.CustomProperties;
            cp["RewardResult"] = winner;
            PhotonNetwork.CurrentRoom.SetCustomProperties(cp);

            PhotonNetwork.LoadLevel("NextStageScene");
        }
    }
}
