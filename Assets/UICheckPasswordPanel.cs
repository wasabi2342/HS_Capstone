using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UICheckPasswordPanel : UIBase
{
    [SerializeField]
    private InputField passwordInputField;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;

    private RoomInfo currentRoomInfo;
    private Action<bool> onResult;


    public void Init(RoomInfo roomInfo, Action<bool> callback)
    {
        currentRoomInfo = roomInfo;
        onResult = callback;

        passwordInputField.text = "";

        confirmButton.onClick.AddListener(OnClickedConfirmButton);
        cancelButton.onClick.AddListener(OnCancelButton);
    }

    private void OnClickedConfirmButton()
    {
        string enteredPassword = passwordInputField.text;
        string correctPassword = (string)currentRoomInfo.CustomProperties["Password"];

        onResult?.Invoke(enteredPassword == correctPassword);
    }

    private void OnCancelButton()
    {
        UIManager.Instance.ClosePeekUI();
    }
}
