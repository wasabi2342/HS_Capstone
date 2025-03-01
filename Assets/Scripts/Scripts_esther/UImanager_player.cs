using UnityEngine;
using UnityEngine.UI;  // Unity UI ���� ���ӽ����̽� ���

public class UIManager_player : MonoBehaviour
{
    public static UIManager_player Instance { get; private set; }

    [Header("Pause / Menu ����")]
    public GameObject pauseMenuPanel;
    public Button quitButton;
    public Button lobbyButton;

    [Header("��ȭ �г� ����")]
    public GameObject dialoguePanel;
    public UnityEngine.UI.Text dialogueText;  // UnityEngine.UI.Text�� ��������� ���
    public string[] npcDialogues;

    private bool isDialogueActive = false;
    private int currentDialogueIndex = 0;

    private void Awake()
    {
        // �̱��� ����: ���� �� ���� �����ϵ���
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
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButton);
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(OnLobbyButton);
    }

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
        Debug.Log("Quit ��ư Ŭ�� (���� ����)");
        // Application.Quit(); // �ʿ� �� ���
    }

    private void OnLobbyButton()
    {
        Debug.Log("�κ�� �̵� (���� ����)");
    }

    // ��ȭ ���� �޼���
    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
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
