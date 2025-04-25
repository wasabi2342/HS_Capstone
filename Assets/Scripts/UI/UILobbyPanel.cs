using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyPanel : UIBase
{
    [Header("패널 선택 버튼")]
    [SerializeField]
    private Button showCreateRoomPanelButton;
    [SerializeField]
    private Button showRoomListPanelButton;
    [SerializeField]
    private Button showSearchRoomPanelButton;

    [Header("룸 리스트 패널")]
    [SerializeField]
    private RectTransform roomButtonParent;
    [SerializeField]
    private Button roomButton1;
    [SerializeField]
    private Button roomButton2;
    [SerializeField]
    private Button preButton1;

    [Header("패널")]
    [SerializeField]
    private RectTransform roomListPanel;
    [SerializeField]
    private RectTransform createRoomPanel;
    [SerializeField]
    private RectTransform searchRoomPanel;

    [Header("검색 패널 UI")]
    [SerializeField]
    private InputField searchRoomInputField;
    [SerializeField]
    private Button searchRoomButton;
    [SerializeField]
    private Button preButton2;

    private List<Button> joinRoomButtonList = new List<Button>();

    private Dictionary<string, RoomInfo> cachedRoomDict = new Dictionary<string, RoomInfo>();

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
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList || info.PlayerCount == 0)
            {
                if (cachedRoomDict.ContainsKey(info.Name))
                {
                    cachedRoomDict.Remove(info.Name);
                }
            }
            else
            {
                cachedRoomDict[info.Name] = info; // 새로 추가하거나 기존 항목 업데이트
            }
        }

        UpdateRoomListUI();
    }

    private void UpdateRoomListUI()
    {
        Debug.Log("룸리스트 수: " + cachedRoomDict.Count);

        foreach (var kvp in cachedRoomDict)
        {
            Debug.Log("룸이름: " + kvp.Key);
        }

        // 기존 버튼 삭제
        foreach (Button button in joinRoomButtonList)
        {
            Destroy(button.gameObject);
            Debug.Log("버튼 삭제");
        }

        joinRoomButtonList.Clear();

        int index = 0;

        foreach (RoomInfo room in cachedRoomDict.Values)
        {
            RoomInfo capturedRoom = room;

            // 홀수/짝수에 따라 프리팹 선택
            Button buttonPrefab = (index % 2 == 0) ? roomButton1 : roomButton2;

            // 버튼 생성
            Button newJoinRoomButton = Instantiate(buttonPrefab, roomButtonParent).GetComponent<Button>();
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(capturedRoom));
            newJoinRoomButton.GetComponentInChildren<Text>().text =
                $"방 이름: {capturedRoom.Name}   {capturedRoom.PlayerCount}/{capturedRoom.MaxPlayers}";

            joinRoomButtonList.Add(newJoinRoomButton);
            Debug.Log("버튼 생성");

            index++;
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
        createRoomPanel.transform.SetAsLastSibling();
    }

    private void OnClickedPreButton()
    {
        PhotonNetwork.Disconnect();
        UIManager.Instance.OpenPanel<UiStartPanel>();
    }

    private void OnClickedSearchRoomButton()
    {
        searchRoomPanel.transform.SetAsLastSibling();
    }

    private void OnClickedShowRoomListButton()
    {
        roomListPanel.transform.SetAsLastSibling();
    }

    public override void Init()
    {
        if (PhotonNetwork.InLobby)
        {
            //PhotonNetwork.GetCustomRoomList(TypedLobby.Default, "");
        }

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
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("방 이름을 입력해주세요.");

            return;
        }

        if (cachedRoomDict.ContainsKey(targetName))
        {
            JoinRoom(cachedRoomDict[targetName]);
        }
        else
        {
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("해당 이름의 방이 존재하지 않습니다.");
        }
    }
}
