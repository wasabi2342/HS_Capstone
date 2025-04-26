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
    private Button roomButton1;
    [SerializeField]
    private Button roomButton2;
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
                cachedRoomDict[info.Name] = info; // ���� �߰��ϰų� ���� �׸� ������Ʈ
            }
        }

        UpdateRoomListUI();
    }

    private void UpdateRoomListUI()
    {
        Debug.Log("�븮��Ʈ ��: " + cachedRoomDict.Count);

        foreach (var kvp in cachedRoomDict)
        {
            Debug.Log("���̸�: " + kvp.Key);
        }

        // ���� ��ư ����
        foreach (Button button in joinRoomButtonList)
        {
            Destroy(button.gameObject);
            Debug.Log("��ư ����");
        }

        joinRoomButtonList.Clear();

        int index = 0;

        foreach (RoomInfo room in cachedRoomDict.Values)
        {
            RoomInfo capturedRoom = room;

            // Ȧ��/¦���� ���� ������ ����
            Button buttonPrefab = (index % 2 == 0) ? roomButton1 : roomButton2;

            // ��ư ����
            Button newJoinRoomButton = Instantiate(buttonPrefab, roomButtonParent).GetComponent<Button>();
            newJoinRoomButton.onClick.AddListener(() => JoinRoom(capturedRoom));
            newJoinRoomButton.GetComponentInChildren<Text>().text =
                $"�� �̸�: {capturedRoom.Name}   {capturedRoom.PlayerCount}/{capturedRoom.MaxPlayers}";

            joinRoomButtonList.Add(newJoinRoomButton);
            Debug.Log("��ư ����");

            index++;
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
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("�� �̸��� �Է����ּ���.");

            return;
        }

        if (cachedRoomDict.ContainsKey(targetName))
        {
            JoinRoom(cachedRoomDict[targetName]);
        }
        else
        {
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("�ش� �̸��� ���� �������� �ʽ��ϴ�.");
        }
    }
}
