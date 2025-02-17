using NUnit.Framework.Constraints;
using Photon.Realtime;
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
    private List<Button> gameLevelButtons;
    [SerializeField]
    private Toggle isVisibleToggle;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;

    private int maxPlayer;
    private string difficulty;

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

        foreach (Button button in gameLevelButtons)
        {
            button.onClick.AddListener(() => OnclickedGameLevelButton(button.GetComponentInChildren<Text>().text, button));
        }
    }

    private void OnClickedCancelButton()
    {
        UIManager.Instance.ClosePeekUI();
    }

    private void OnClickedConfirmButton()
    {
        if (roomNameInputField.text != "" && maxPlayer != 0 && difficulty !="")
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayer;
            roomOptions.IsVisible = !isVisibleToggle.isOn;
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "Difficulty", $"{difficulty}" }
            };
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

    private void OnclickedGameLevelButton(string difficulty, Button button)
    {
        this.difficulty = difficulty;

        foreach (Button btn in gameLevelButtons)
        {
            btn.interactable = true;
        }
        
        button.interactable = false;
    }
}
