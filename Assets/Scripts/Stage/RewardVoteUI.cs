using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// 씬 어딘가에 배치 (단 하나만 존재)
public class RewardVoteUI : MonoBehaviour
{
    public static RewardVoteUI Instance;

    private bool isCreated = false;  // UI를 한 번만 생성하도록 관리
    private Canvas mainCanvas;
    private RectTransform canvasRect;
    private GameObject votePanel;    // 패널 루트

    private void Awake()
    {
        Instance = this;
    }

    // DoorInteraction에서 RPC_ShowVoteUI 호출 시 여기로 옴
    public void ShowVoteUI()
    {
        if (!isCreated)
        {
            CreateCanvasAndEventSystem();
            CreateVotePanel();
            isCreated = true;
        }

        // 패널 활성화
        votePanel.SetActive(true);
    }

    private void CreateCanvasAndEventSystem()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("VoteCanvas");
        canvasObj.layer = LayerMask.NameToLayer("UI");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasRect = mainCanvas.GetComponent<RectTransform>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // EventSystem 없으면 생성
        if (!FindObjectOfType<EventSystem>())
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private void CreateVotePanel()
    {
        // 패널 만들기
        votePanel = new GameObject("VotePanel");
        votePanel.transform.SetParent(canvasRect, false);

        RectTransform rt = votePanel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 300);
        rt.anchoredPosition = Vector2.zero;

        Image bg = votePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        // 간단히 A/B 버튼 붙이거나, 노드식 UI를 생성
        // 여기서는 텍스트와 버튼 2개만 예시
        CreateTextMeshPro("VoteTitle", votePanel.transform as RectTransform, "어떤 보상을 선택할까?");
        CreateVoteButton("VoteA", "A 보상", new Vector2(-80, -50), () => OnClickVote("A"));
        CreateVoteButton("VoteB", "B 보상", new Vector2(80, -50), () => OnClickVote("B"));

        votePanel.SetActive(false);
    }

    private void CreateTextMeshPro(string name, RectTransform parent, string text)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 50);
        rt.anchoredPosition = new Vector2(0, 60);

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private void CreateVoteButton(string name, string btnText, Vector2 pos, UnityEngine.Events.UnityAction callback)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(votePanel.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 50);
        rt.anchoredPosition = pos;

        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(callback);

        // 텍스트
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.sizeDelta = new Vector2(120, 50);
        txtRT.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = btnText;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
    }

    private void OnClickVote(string choice)
    {
        Debug.Log($"내가 {choice}에 투표!");
        // 여기서 Photon RPC로 마스터에게 “내 투표” 알리거나,
        // 바로 씬 전환, etc.
        votePanel.SetActive(false);
    }
}
