using System.Collections.Generic;
using UnityEngine;

public enum UIState
{
    Start,
    Lobby,
    Room,
    CreateRoom
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Dictionary<UIState, RectTransform> uiPanels = new Dictionary<UIState, RectTransform>();
    private Stack<UIState> uiStack = new Stack<UIState>();

    [SerializeField]
    private RectTransform startUI;
    [SerializeField]
    private RectTransform lobbyUI;
    [SerializeField]
    private RectTransform roomUI;
    [SerializeField]
    private RectTransform createRoomUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        uiPanels[UIState.Start] = startUI;
        uiPanels[UIState.Lobby] = lobbyUI;
        uiPanels[UIState.CreateRoom] = createRoomUI;

        OpenPanel(UIState.Start);
    }

    public void OpenPanel(UIState newPanel)
    {
        if (uiStack.Count > 0)
        {
            uiPanels[uiStack.Peek()].gameObject.SetActive(false);
        }

        uiPanels[newPanel].gameObject.SetActive(true);
        uiStack.Push(newPanel);
    }

    public void GoBack()
    {
        if (uiStack.Count > 1)
        {
            uiPanels[uiStack.Pop()].gameObject.SetActive(false);

            uiPanels[uiStack.Peek()].gameObject.SetActive(true);
        }
    }

    public void OpenPopupPanel(UIState newPanel)
    {
        uiPanels[newPanel].gameObject.SetActive(true);
        uiStack.Push(newPanel);
    }
}
