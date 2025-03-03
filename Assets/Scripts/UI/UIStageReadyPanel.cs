using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        StartCoroutine(TimeCount());
    }

    private IEnumerator TimeCount()
    {
        int i = 60;
        while (i >= 0)
        {
            yield return new WaitForSeconds(1);
            timeText.text = "제한시간 " + i;
            i--;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && RoomManager.Instance.isEnteringStage)
        {
            PhotonNetworkManager.Instance.ReadyToEnterStage();
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

    protected override void OnDisable()
    {
        base.OnDisable();
        PhotonNetworkManager.Instance.OnUpdateReadyPlayer -= UpdateToggls;
    }

    public void UpdateToggls(int readyPlayerNum)
    {
        for(int i = 0; i < readyPlayerNum; i++)
        {
            toggles[i].isOn = true;
        }
    }
}
