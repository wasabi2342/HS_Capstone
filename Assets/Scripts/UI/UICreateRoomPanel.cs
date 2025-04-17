using NUnit.Framework.Constraints;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICreateRoomPanel : UIBase
{
    [SerializeField]
    private InputField roomNameInputField;
    [SerializeField]
    private List<Button> maxPlayerButtons;
    [SerializeField]
    private Toggle isVisibleToggle;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;
    [SerializeField]
    private InputField passwordInputField;

    private int maxPlayer;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        cancelButton.onClick.AddListener(OnClickedCancelButton);
        confirmButton.onClick.AddListener(OnClickedConfirmButton);
        foreach (Button button in maxPlayerButtons)
        {
            button.onClick.AddListener(() => OnClickedMaxPlayerButton(int.Parse(button.GetComponentInChildren<Text>().text), button));
        }
        isVisibleToggle.onValueChanged.AddListener(OnisVisibleToggleChanged);
        passwordInputField.gameObject.SetActive(false); 
    }

    private void OnisVisibleToggleChanged(bool isOn)
    {
        if (isOn)
        {
            passwordInputField.gameObject.SetActive(true);
        }
        else
        {
            passwordInputField.gameObject.SetActive(false);
        }
    }

    private void OnClickedCancelButton()
    {
        PhotonNetwork.Disconnect();
        UIManager.Instance.OpenPanel<UiStartPanel>();
    }

    private void OnClickedConfirmButton()
    {
        if (roomNameInputField.text != "" && maxPlayer != 0)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayer;
            if (isVisibleToggle.isOn)
            {
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "Password", passwordInputField.text }
                };
            }

            // 비밀번호를 로비에서 볼 수 있게 설정
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "Password" };

            PhotonNetworkManager.Instance.CreateRoom(roomNameInputField.text, roomOptions);
        }
        else
        {
            UIManager.Instance.OpenPopupPanel<UIDialogPanel>().SetInfoText("내용을 입력해 주세요.");
        }
    }

    private void OnClickedMaxPlayerButton(int num, Button button)
    {
        maxPlayer = num;

        foreach (Button btn in maxPlayerButtons)
        {
            btn.interactable = true;
        }

        button.interactable = false;
    }
}
