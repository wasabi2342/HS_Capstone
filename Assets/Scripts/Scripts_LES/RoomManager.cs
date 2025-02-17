using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviourPun
{
    [SerializeField]
    private GameObject playerInRoom;

    public static RoomManager Instance {  get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.Instantiate(playerInRoom.name, Vector3.zero, Quaternion.identity);
        isEnteringStage = false;
    }

    public void InteractWithDungeonNPC()
    {
        UIManager.Instance.OpenPopupPanel<UIConfirmPanel>().Init(
            () => WaitForEnterStage(), 
            () => UIManager.Instance.ClosePeekUI(), 
            "게임 스테이지에 진입하시겠습니까?"
            );
    }

    public void WaitForEnterStage()
    {
        UIManager.Instance.ClosePeekUI();
        PhotonNetworkManager.Instance.ReadyToEnterStage();
    }

    public void EnterRestrictedArea(int viewID)
    {
        if (playerInRestrictedArea.ContainsKey(viewID))
        {
            playerInRestrictedArea[viewID] = true;
        }
        else
        {
            playerInRestrictedArea.Add(viewID, true);
        }
    }

    public void ExitRestrictedArea(int viewID)
    {
        if (playerInRestrictedArea.ContainsKey(viewID))
        {
            playerInRestrictedArea[viewID] = false;
        }
        else
        {
            playerInRestrictedArea.Add(viewID, false);
        }
    }

    public bool IsPlayerInRestrictedArea()
    {
        return playerInRestrictedArea.ContainsValue(true);
    }
}
