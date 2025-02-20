using UnityEngine;
using UnityEngine.UI;

public class UIManager_player : MonoBehaviour
{
    public static UIManager_player Instance { get; private set; }

    [Header("Pause / Menu 관련")]
    public GameObject pauseMenuPanel;
    public Button quitButton;
    public Button lobbyButton;

    [Header("대화 패널 관련")]
    public GameObject dialoguePanel;
    public UnityEngine.UI.Text dialogueText;  // 여기서 명시적으로 UnityEngine.UI.Text 사용
    public string[] npcDialogues;

    // 내부 상태 관리
    private bool isDialogueActive = false;
    private int currentDialogueIndex = 0;

    private void Awake()
    {
        // 싱글턴 패턴 (씬에 한 개만 존재하도록)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 UI 상태 설정
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 버튼 이벤트 연결
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButton);
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(OnLobbyButton);
    }

    // Pause 메뉴 토글
    public void TogglePauseMenu()
    {
        if (pauseMenuPanel == null)
            return;
        bool isActive = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isActive);
    }

    private void OnQuitButton()
    {
        TogglePauseMenu();
        Debug.Log("Quit 버튼 클릭 (추후 구현)");
        // Application.Quit(); // 필요 시 사용
    }

    private void OnLobbyButton()
    {
        Debug.Log("로비로 이동 (추후 구현)");
    }

    // 대화 관련
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void StartDialogue()
    {
        if (dialoguePanel == null || dialogueText == null || npcDialogues == null)
            return;

        isDialogueActive = true;
        currentDialogueIndex = 0;
        dialoguePanel.SetActive(true);

        if (npcDialogues.Length > 0)
            dialogueText.text = npcDialogues[currentDialogueIndex];
    }

    public void NextDialogue()
    {
        if (!isDialogueActive)
            return;

        currentDialogueIndex++;
        if (currentDialogueIndex < npcDialogues.Length)
        {
            dialogueText.text = npcDialogues[currentDialogueIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        currentDialogueIndex = 0;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}
