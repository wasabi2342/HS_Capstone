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

    private List<Button> joinRoomButtonList = new List<Button>();

    private void Start()
    {
        Init();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetworkManager.Instance.OnRoomListUpdated -= UpdateRoomList;
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Button button in joinRoomButtonList)
        {
            Destroy(button);
        }

        joinRoomButtonList.Clear();

        foreach (RoomInfo room in roomList)
        {
            Button newJoinRoomButton = Instantiate(roomButton, roomButtonParent);
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(room.Name));
            newJoinRoomButton.GetComponentInChildren<Text>().text = $"πÊ ¿Ã∏ß: {room.Name}   {room.PlayerCount}/{room.MaxPlayers}";
            joinRoomButtonList.Add(newJoinRoomButton);
        }
    }

    private void JoinRoom(string roomName)
    {
        PhotonNetworkManager.Instance.JoinRoom(roomName);
    }

    private void OnClickedCreateRoomButton()
    {
        UIManager.Instance.OpenPopupPanel<UICreateRoomPanel>();
    }

    public override void Init()
    {
        createRoomButton.onClick.AddListener(OnClickedCreateRoomButton);
        PhotonNetworkManager.Instance.OnRoomListUpdated += UpdateRoomList;
    }
}
