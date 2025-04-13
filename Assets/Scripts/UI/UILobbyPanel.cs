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
    private Button joinRoomButton;
    [SerializeField]
    private InputField roomNameField;
    [SerializeField]
    private RectTransform roomButtonParent;
    [SerializeField]
    private Button roomButton; 
    [SerializeField]
    private Button preButton;

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
            // 昏力等 规篮 公矫
            if (room.RemovedFromList || room.PlayerCount == 0)
                continue;

            Button newJoinRoomButton = Instantiate(roomButton, roomButtonParent);
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(room.Name));
            newJoinRoomButton.GetComponentInChildren<Text>().text =
                $"规 捞抚: {room.Name}   {room.PlayerCount}/{room.MaxPlayers}";
            joinRoomButtonList.Add(newJoinRoomButton);
        }
    }

    private void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        UIManager.Instance.CloseAllUI();
        UIManager.Instance.OpenPopupPanel<UIRoomPanel>();
    }

    private void OnClickedCreateRoomButton()
    {
        UIManager.Instance.OpenPopupPanel<UICreateRoomPanel>();
    }

    private void OnClickedPreButton()
    {
        UIManager.Instance.OpenPanel<UiStartPanel>();
    }

    public override void Init()
    {
        createRoomButton.onClick.AddListener(OnClickedCreateRoomButton);
        preButton.onClick.AddListener(OnClickedPreButton);
    }
}
