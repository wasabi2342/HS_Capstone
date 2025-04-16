using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPanel : UIBase
{
    [Header("�г� ���� ��ư")]
    [SerializeField]
    private Button showCreateRoomPanelButton;
    [SerializeField]
    private Button showRoomListPanelButton;
    [SerializeField]
    private Button showSearchRoomPanelButton;

    [Header("�� ����Ʈ �г�")]
    [SerializeField]
    private RectTransform roomButtonParent;
    [SerializeField]
    private Button roomButton;
    [SerializeField]
    private Button preButton1;

    [Header("�г�")] 
    [SerializeField]
    private RectTransform roomListPanel;
    [SerializeField]
    private RectTransform createRoomPanel;
    [SerializeField]
    private RectTransform searchRoomPanel;

    [Header("�˻� �г� UI")]
    [SerializeField]
    private InputField searchRoomInputField;
    [SerializeField]
    private Button searchRoomButton;
    [SerializeField]
    private Button preButton2;

    private List<Button> joinRoomButtonList = new List<Button>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Start()
    {
        Init();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        this.roomList = roomList;

        foreach (Button button in joinRoomButtonList)
        {
            Destroy(button.gameObject);
        }

        joinRoomButtonList.Clear();

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList || room.PlayerCount == 0)
                continue;

            RoomInfo capturedRoom = room; // ���纻 ���

            Button newJoinRoomButton = Instantiate(roomButton, roomButtonParent);
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(capturedRoom));
            newJoinRoomButton.GetComponentInChildren<Text>().text =
                $"�� �̸�: {capturedRoom.Name}   {capturedRoom.PlayerCount}/{capturedRoom.MaxPlayers}";

            joinRoomButtonList.Add(newJoinRoomButton);
        }
    }

    private void JoinRoom(RoomInfo room)
    {
        // ��й�ȣ�� �ִ��� Ȯ��
        if (room.CustomProperties.ContainsKey("Password"))
        {
            UIManager.Instance.OpenPopupPanel<UICheckPasswordPanel>().Init(room, isCorrect =>
            {
                if (isCorrect)
                {
                    PhotonNetwork.JoinRoom(room.Name);
                }
                else
                {
                    UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("��й�ȣ�� Ʋ�Ƚ��ϴ�.");
                }
            });
        }
        else
        {
            PhotonNetwork.JoinRoom(room.Name);
        }
    }



    public override void OnJoinedRoom()
    {
        UIManager.Instance.CloseAllUI();
        UIManager.Instance.OpenPopupPanel<UIRoomPanel>();
    }

    private void OnClickedCreateRoomButton()
    {
        roomListPanel.gameObject.SetActive(false);
        searchRoomPanel.gameObject.SetActive(false);
        createRoomPanel.gameObject.SetActive(true);

        showCreateRoomPanelButton.gameObject.SetActive(false);
        showRoomListPanelButton.gameObject.SetActive(true);
        showSearchRoomPanelButton.gameObject.SetActive(true);
    }

    private void OnClickedPreButton()
    {
        PhotonNetwork.Disconnect();
        UIManager.Instance.OpenPanel<UiStartPanel>();
    }

    private void OnClickedSearchRoomButton()
    {
        roomListPanel.gameObject.SetActive(false);
        searchRoomPanel.gameObject.SetActive(true);
        createRoomPanel.gameObject.SetActive(false);

        showCreateRoomPanelButton.gameObject.SetActive(true);
        showRoomListPanelButton.gameObject.SetActive(true);
        showSearchRoomPanelButton.gameObject.SetActive(false);
    }

    private void OnClickedShowRoomListButton()
    {
        roomListPanel.gameObject.SetActive(true);
        searchRoomPanel.gameObject.SetActive(false);
        createRoomPanel.gameObject.SetActive(false);

        showCreateRoomPanelButton.gameObject.SetActive(true);
        showRoomListPanelButton.gameObject.SetActive(false);
        showSearchRoomPanelButton.gameObject.SetActive(true);
    }

    public override void Init()
    {
        showCreateRoomPanelButton.onClick.AddListener(OnClickedCreateRoomButton);
        showRoomListPanelButton.onClick.AddListener(OnClickedShowRoomListButton);
        showSearchRoomPanelButton.onClick.AddListener(OnClickedSearchRoomButton);

        preButton1.onClick.AddListener(OnClickedPreButton);
        preButton2.onClick.AddListener(OnClickedPreButton);
        searchRoomButton.onClick.AddListener(TryJoinRoomByName);

        OnClickedShowRoomListButton();
    }

    private void TryJoinRoomByName()
    {
        string targetName = searchRoomInputField.text.Trim();

        if (string.IsNullOrEmpty(targetName))
        {
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("�� �̸��� �Է����ּ���.");

            return;
        }

        // ���� �̸��� ���� �ִ��� Ȯ��
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                continue;

            if (room.Name == targetName)
            {
                JoinRoom(room);
                return;
            }
        }

        UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("�ش� �̸��� ���� �������� �ʽ��ϴ�.");
    }
}
