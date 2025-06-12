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
        for (int i = 0; i < maxPlayerButtons.Count; i++)
        {
            int maxPlayers = i + 2; // �ο����� 2���� ����
            Button button = maxPlayerButtons[i];
            button.onClick.AddListener(() => OnClickedMaxPlayerButton(maxPlayers, button));
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
        UIManager.Instance.OpenPanelInOverlayCanvas<UiStartPanel>();
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

            // ��й�ȣ�� �κ񿡼� �� �� �ְ� ����
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "Password" };

            PhotonNetworkConnectManager.Instance.CreateRoom(roomNameInputField.text, roomOptions);
        }
        else
        {
            UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("������ �Է��� �ּ���.");
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
