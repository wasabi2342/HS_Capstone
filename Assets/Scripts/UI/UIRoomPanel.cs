using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UIRoomPanel : UIBase
{
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private UIPlayerInfoPanel playerInfoPanel;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private TextMeshProUGUI roomName;

    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private Animator characterImageAnimator;
    [SerializeField]
    private Button rightArrowButton;
    [SerializeField]
    private Button leftArrowButton;

    [SerializeField]
    private TextMeshProUGUI characterInfoText;
    [SerializeField]
    private Button[] skillIcons = new Button[5];
    [SerializeField]
    private TextMeshProUGUI skillInfoTest;
    [SerializeField]
    private RawImage skillRawImage;


    [SerializeField]
    private Image readyImage;
    [SerializeField]
    private Sprite readySprite;
    [SerializeField]
    private Sprite unReadySprite;
    [SerializeField]
    private TextMeshProUGUI readyText;

    private Dictionary<int, UIPlayerInfoPanel> players = new Dictionary<int, UIPlayerInfoPanel>();

    private bool isReady;
    private bool canStart;
    private readonly string DefeaultCharacter = "WhitePlayer";

    private int selectIndex = 0;

    private Action<InputAction.CallbackContext> exitRoom;

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);

        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null)
        {
            InputManager.Instance.PlayerInput.actions["ESC"].performed -= exitRoom;
        }
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
        exitRoom = OnClickedPreButton;

        InputManager.Instance.PlayerInput.actions["ESC"].performed += exitRoom;

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "시작";
            isReady = true;
        }
        else
        {
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "준비";
            isReady = false;
        }

        if (PhotonNetwork.InRoom)
        {
            roomName.text = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            roomName.text = "싱글플레이";
        }

        startButton.onClick.AddListener(OnClickedStartButton);

        canStart = true;
        canStart = canStart && isReady;

        UIPlayerInfoPanel myPanel = Instantiate(playerInfoPanel);
        myPanel.transform.SetParent(content.transform, false);

        myPanel.Init(isReady, PhotonNetwork.LocalPlayer.NickName, DefeaultCharacter, PhotonNetwork.IsMasterClient);

        characterInfoText.text = ((Characters)(selectIndex % (int)Characters.Max)).ToString() + "의 정보";

        //characterImage.sprite = Resources.Load<Sprite>("Sprite/" + ((Characters)(selectIndex % (int)Characters.Max)).ToString());
        characterImageAnimator.SetInteger("Index", (selectIndex % (int)Characters.Max));

        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"SkillIcon/SkillInfo/{(Characters)(selectIndex % (int)Characters.Max)}/{(Skills)i}_deactivate");
            int index = i; // 지역 변수로 캡처
            skillIcons[i].onClick.AddListener(() => OnClickedSkillIconButton(index));
        }

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
                    panel.Init(isReady, otherPlayer.NickName, character, kvp.Value.IsMasterClient);
                }
                else
                {
                    UIPlayerInfoPanel newPanel = Instantiate(playerInfoPanel);
                    newPanel.transform.SetParent(content.transform, false);
                    newPanel.Init(isReady, otherPlayer.NickName, character, kvp.Value.IsMasterClient);
                    players.Add(otherPlayer.ActorNumber, newPanel);
                }
            }
        }
        else
        {
            players.Add(-1, myPanel);
        }

        leftArrowButton.onClick.AddListener(() => OnClickedArrowButton(-1));
        rightArrowButton.onClick.AddListener(() => OnClickedArrowButton(1));
    }

    private void OnClickedSkillIconButton(int index)
    {
        skillInfoTest.text = DataManager.Instance.FindSkillByBlessingKeyAndCharacter(index, 0, (selectIndex % (int)Characters.Max)).Bless_Discript;

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i == index)
            {
                skillIcons[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"SkillIcon/SkillInfo/{(Characters)(selectIndex % (int)Characters.Max)}/{(Skills)i}_activate");
            }
            else
            {
                skillIcons[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"SkillIcon/SkillInfo/{(Characters)(selectIndex % (int)Characters.Max)}/{(Skills)i}_deactivate");
            }
        }
    }

    private void OnClickedArrowButton(int num)
    {
        skillInfoTest.text = "";

        selectIndex += num;

        selectIndex = Mathf.Abs(selectIndex);

        //characterImage.sprite = Resources.Load<Sprite>("Sprite/" + (Characters)(selectIndex % (int)Characters.Max));
        characterImageAnimator.SetInteger("Index", (selectIndex % (int)Characters.Max));

        characterInfoText.text = ((Characters)(selectIndex % (int)Characters.Max)).ToString() + "의 정보";

        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].GetComponent<Image>().sprite = Resources.Load<Sprite>($"SkillIcon/SkillInfo/{(Characters)(selectIndex % (int)Characters.Max)}/{(Skills)i}_deactivate");
            //int index = i; // 지역 변수로 캡처
            //skillIcons[i].onClick.AddListener(() => OnClickedSkillIconButton(index));
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", isReady },
                { "SelectCharacter", ((Characters)(selectIndex % (int)Characters.Max)).ToString() }
            });

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
            panel.Init(isReady, targetPlayer.NickName, character, targetPlayer.IsMasterClient);
        }
        else
        {
            UIPlayerInfoPanel newPanel = Instantiate(playerInfoPanel);
            newPanel.transform.SetParent(content.transform, false);
            newPanel.Init(isReady, targetPlayer.NickName, character, targetPlayer.IsMasterClient);
            players.Add(targetPlayer.ActorNumber, newPanel);
        }

        CheckCanStart();
    }

    public void OnClickedPreButton(InputAction.CallbackContext ctx)
    {
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIConfirmPanel>().Init(() =>
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                UIManager.Instance.OpenPanelInOverlayCanvas<UiStartPanel>();
            }
        },
        () =>
        {
            if(UIManager.Instance.ReturnPeekUI() as UIConfirmPanel)
            {
                UIManager.Instance.ClosePeekUI();
            }
        },
        "방을 나가시겠습니까?"
        );
    }

    public void OnClickedStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (canStart)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
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

            characterImageAnimator.SetBool("ready", isReady);

            readyImage.sprite = isReady ? readySprite : unReadySprite;
            readyText.text = isReady ? "READY!" : "READY";

            leftArrowButton.interactable = !isReady;
            rightArrowButton.interactable = !isReady;
        }
    }

    public override void OnLeftRoom()
    {
        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.JoinLobby();
            UIManager.Instance.OpenPanelInOverlayCanvas<UILobbyPanel>();
        }
        else
        {
            UIManager.Instance.OpenPanelInOverlayCanvas<UiStartPanel>();
        }
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
        players[-1].Init(true, PlayerPrefs.GetString("Nickname"), characterName, true);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            // 마스터 클라이언트가 나 자신이면 버튼 텍스트와 상태 변경
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "시작";
            isReady = true;

            // 내 패널 상태도 갱신
            if (PhotonNetwork.InRoom && players.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out var panel))
            {
                panel.Init(isReady, PhotonNetwork.LocalPlayer.NickName, (string)PhotonNetwork.LocalPlayer.CustomProperties["SelectCharacter"], PhotonNetwork.IsMasterClient);
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
