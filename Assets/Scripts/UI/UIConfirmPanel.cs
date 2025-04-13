using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmPanel : UIBase
{
    [SerializeField]
    private Text message;
    [SerializeField]
    private Button confirmButton;
    [SerializeField]
    private Button cancelButton;

    public void Init(Action confirmEvent, Action cancelEvent, string message)
    {
        if (confirmEvent == null)
        {
            Debug.Log("�׼� ��");
        }
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => confirmEvent.Invoke());
        cancelButton.onClick.AddListener(() => cancelEvent.Invoke());
        this.message.text = message;
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }
}
