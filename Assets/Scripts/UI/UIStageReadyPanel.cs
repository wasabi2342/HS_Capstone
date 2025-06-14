using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIStageReadyPanel : UIBase
{
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private RectTransform toggleParent;
    [SerializeField]
    private Toggle toggle;

    private int maxPlayer;
    private List<Toggle> toggles = new List<Toggle>();

    private System.Action<InputAction.CallbackContext> stageEnterConfirmAction;

    private void Start()
    {
        StartCoroutine(TimeCount());

        stageEnterConfirmAction = Ready;
        InputManager.Instance.PlayerInput.actions["StageEnterConfirm"].performed += stageEnterConfirmAction;
    }


    public void Ready(InputAction.CallbackContext ctx)
    {
        if (RoomManager.Instance.isEnteringStage)
        {
            PhotonNetworkManager.Instance.ReadyToEnterStage();
        }
    }

    private IEnumerator TimeCount()
    {
        int i = 60;
        while (i >= 0)
        {
            yield return new WaitForSeconds(1);
            timeText.text = "���ѽð� " + i;
            i--;
        }
    }

    public override void Init()
    {
        maxPlayer = PhotonNetwork.CurrentRoom.PlayerCount;

        for (int i = 0; i < maxPlayer; i++)
        { 
            toggles.Add(Instantiate(toggle, toggleParent));
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetworkManager.Instance.OnUpdateReadyPlayer -= UpdateToggls;

        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && stageEnterConfirmAction != null)
        {
            InputManager.Instance.PlayerInput.actions["StageEnterConfirm"].performed -= stageEnterConfirmAction;
        }
    }

    public void UpdateToggls(int readyPlayerNum)
    {
        for(int i = 0; i < readyPlayerNum; i++)
        {
            toggles[i].isOn = true;
        }
    }
}
