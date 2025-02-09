using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICreateRoomPanel : MonoBehaviour
{
    [SerializeField]
    private InputField roomNameInputField;
    [SerializeField]
    private List<Button> maxPlayerButtons;
    [SerializeField]
    private List<Button> gameLevelButtons;
    [SerializeField]
    private Toggle isVisibleToggle;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;

    private int maxPlayer;

    void Start()
    {
        cancelButton.onClick.AddListener(OnClickedCancelButton);
        confirmButton.onClick.AddListener(OnClickedConfirmButton);
        foreach(Button button in maxPlayerButtons)
        {
            button.onClick.AddListener(() => OnClickedMaxPlayerButton(int.Parse(button.GetComponentInChildren<Text>().text), button));
        }

        foreach(Button button in gameLevelButtons)
        {
            button.onClick.AddListener(() => OnclickedGameLevelButton(button));
        }
    }

    private void OnClickedCancelButton()
    {
        UIManager.Instance.GoBack();
    }

    private void OnClickedConfirmButton()
    {
        if (roomNameInputField.text != "" && maxPlayer != 0)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayer;
            roomOptions.IsVisible = !isVisibleToggle.isOn;
            PhotonNetworkManager.Instance.CreateRoom(roomNameInputField.text, roomOptions);
            UIManager.Instance.GoBack();
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

    private void OnclickedGameLevelButton(Button button)
    {
        foreach (Button btn in gameLevelButtons)
        {
            btn.interactable = true;
        }
        
        button.interactable = false;
    }

    private void OnDisable()
    {
        maxPlayer = 0;
        isVisibleToggle.isOn = false;
        roomNameInputField.text = "";

        foreach (Button btn in maxPlayerButtons)
        {
            btn.interactable = true;
        }

        foreach (Button btn in gameLevelButtons)
        {
            btn.interactable = true;
        }
    }
}
