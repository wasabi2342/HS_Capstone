using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPanel : UIBase
{
    [SerializeField]
    private Button createRoomButton;
    [SerializeField]
    private Button showRoomListButton;
    [SerializeField]
    private Button searchRoomButton;
    [SerializeField]
    private InputField roomNameField;
    [SerializeField]
    private RectTransform roomButtonParent;
    [SerializeField]
    private Button roomButton;
    [SerializeField]
    private Button preButton;
    [SerializeField]
    private RectTransform roomListPanel;
    [SerializeField]
    private RectTransform createRoomPanel;
    [SerializeField]
    private RectTransform searchRoomPanel;

    private List<Button> joinRoomButtonList = new List<Button>();

    private void Start()
    {
        Init();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Button button in joinRoomButtonList)
        {
            Destroy(button.gameObject);
        }

        joinRoomButtonList.Clear();

        foreach (RoomInfo room in roomList)
        {
            // 삭제된 방은 무시
            if (room.RemovedFromList || room.PlayerCount == 0)
                continue;

            Button newJoinRoomButton = Instantiate(roomButton, roomButtonParent);
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(room));
            newJoinRoomButton.GetComponentInChildren<Text>().text =
                $"방 이름: {room.Name}   {room.PlayerCount}/{room.MaxPlayers}";
            joinRoomButtonList.Add(newJoinRoomButton);
        }
    }

    private void JoinRoom(RoomInfo room)
    {
        // 비밀번호가 있는지 확인
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
                    UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("비밀번호가 틀렸습니다.");
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

        createRoomButton.gameObject.SetActive(false);
        showRoomListButton.gameObject.SetActive(true);
        searchRoomButton.gameObject.SetActive(true);
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

        createRoomButton.gameObject.SetActive(true);
        showRoomListButton.gameObject.SetActive(true);
        searchRoomButton.gameObject.SetActive(false);
    }

    private void OnClickedShowRoomListButton()
    {
        roomListPanel.gameObject.SetActive(true);
        searchRoomPanel.gameObject.SetActive(false);
        createRoomPanel.gameObject.SetActive(false);

        createRoomButton.gameObject.SetActive(true);
        showRoomListButton.gameObject.SetActive(false);
        searchRoomButton.gameObject.SetActive(true);
    }

    public override void Init()
    {
        createRoomButton.onClick.AddListener(OnClickedCreateRoomButton);
        showRoomListButton.onClick.AddListener(OnClickedShowRoomListButton);
        searchRoomButton.onClick.AddListener(OnClickedSearchRoomButton);

        preButton.onClick.AddListener(OnClickedPreButton);

        OnClickedShowRoomListButton();
    }
}
