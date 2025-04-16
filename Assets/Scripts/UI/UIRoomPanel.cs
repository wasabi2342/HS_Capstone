using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIRoomPanel : UIBase
{
    [SerializeField]
    private Button preButton;
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private UIPlayerInfoPanel playerInfoPanel;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private RectTransform container;
    [SerializeField]
    private Text roomName;

    private Dictionary<int, UIPlayerInfoPanel> players = new Dictionary<int, UIPlayerInfoPanel>();

    private bool isReady;
    private bool canStart;
    private readonly string DefeaultCharacter = "WhitePlayer";

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            startButton.GetComponentInChildren<Text>().text = "시작";
            isReady = true;
        }
        else
        {
            startButton.GetComponentInChildren<Text>().text = "준비";
            isReady = false;
        }

        if(PhotonNetwork.InRoom)
        {
            roomName.text = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            roomName.text = "싱글플레이";
        }

        startButton.onClick.AddListener(OnClickedStartButton);
        preButton.onClick.AddListener(OnClickedPreButton);

        canStart = true;
        canStart = canStart && isReady;

        UIPlayerInfoPanel myPanel = Instantiate(playerInfoPanel);
        myPanel.transform.SetParent(content.transform, false);

        myPanel.Init(isReady, PhotonNetwork.LocalPlayer.NickName, DefeaultCharacter, OnClickedSelectCharacterButton);

        if (PhotonNetwork.InRoom)
        {

            players.Add(PhotonNetwork.LocalPlayer.ActorNumber, myPanel);

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", isReady },
                { "SelectCharacter", DefeaultCharacter }
            });

            foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
            {
                Player otherPlayer = kvp.Value;

                if (otherPlayer == PhotonNetwork.LocalPlayer)
                {
                    continue;
                }

                if (!otherPlayer.CustomProperties.ContainsKey("IsReady") || !otherPlayer.CustomProperties.ContainsKey("SelectCharacter"))
                {
                    return;
                }

                bool isReady = (bool)otherPlayer.CustomProperties["IsReady"];
                string character = (string)otherPlayer.CustomProperties["SelectCharacter"];

                if (players.TryGetValue(otherPlayer.ActorNumber, out UIPlayerInfoPanel panel))
                {
                    panel.Init(isReady, otherPlayer.NickName, character);
                }
                else
                {
                    UIPlayerInfoPanel newPanel = Instantiate(playerInfoPanel);
                    newPanel.transform.SetParent(content.transform, false);
                    newPanel.Init(isReady, otherPlayer.NickName, character);
                    players.Add(otherPlayer.ActorNumber, newPanel);
                }
            }
        }
        else
        {
            players.Add(-1, myPanel);
        }
    }

    public void OnClickedSelectCharacterButton()
    {
        container.DOAnchorPos(container.anchoredPosition + new Vector2(-2000f, 0f), 0.5f).OnComplete(() => UIManager.Instance.OpenPopupPanel<UISelectCharacterPanel>().SetRoomPanelContainer(container));
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == null || changedProps == null)
        {
            return;
        }

        if (!targetPlayer.CustomProperties.ContainsKey("IsReady") || !targetPlayer.CustomProperties.ContainsKey("SelectCharacter"))
        {
            return;
        }

        Debug.Log((bool)targetPlayer.CustomProperties["IsReady"]);
        Debug.Log((string)targetPlayer.CustomProperties["SelectCharacter"]);

        bool isReady = (bool)targetPlayer.CustomProperties["IsReady"];
        string character = (string)targetPlayer.CustomProperties["SelectCharacter"];

        if (players.TryGetValue(targetPlayer.ActorNumber, out UIPlayerInfoPanel panel))
        {
            panel.Init(isReady, targetPlayer.NickName, character);
        }
        else
        {
            UIPlayerInfoPanel newPanel = Instantiate(playerInfoPanel);
            newPanel.transform.SetParent(content.transform, false);
            newPanel.Init(isReady, targetPlayer.NickName, character);
            players.Add(targetPlayer.ActorNumber, newPanel);
        }

        CheckCanStart();
    }

    public void OnClickedPreButton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            UIManager.Instance.OpenPanel<UiStartPanel>();
        }
    }

    public void OnClickedStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (canStart)
            {
                PhotonNetwork.LoadLevel("Room");
            }
        }
        else if (!PhotonNetwork.IsConnected)
        {
            if (canStart)
            {
                SceneManager.LoadScene("Room");
            }
        }
        else
        {
            isReady = !isReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", isReady },
            });
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.JoinLobby();
        UIManager.Instance.OpenPanel<UILobbyPanel>();
    }

    private void CheckCanStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        canStart = true;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!(bool)player.CustomProperties["IsReady"])
            {
                canStart = false;
                break;
            }
        }

        startButton.interactable = canStart;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (players.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(players[otherPlayer.ActorNumber].gameObject);
            players.Remove(otherPlayer.ActorNumber);
        }

        CheckCanStart();
    }

    public void UpdateMyCharacterImage(string characterName)
    {
        players[-1].Init(true, PlayerPrefs.GetString("Nickname"), characterName);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            // 마스터 클라이언트가 나 자신이면 버튼 텍스트와 상태 변경
            startButton.GetComponentInChildren<Text>().text = "시작";
            isReady = true;

            // 내 패널 상태도 갱신
            if (PhotonNetwork.InRoom && players.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out var panel))
            {
                panel.Init(isReady, PhotonNetwork.LocalPlayer.NickName, (string)PhotonNetwork.LocalPlayer.CustomProperties["SelectCharacter"]);
            }

            // CustomProperties에도 반영
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", isReady }
            });

            CheckCanStart();
        }
    }
}
