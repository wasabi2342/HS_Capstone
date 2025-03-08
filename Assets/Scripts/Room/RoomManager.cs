using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerInRoom;
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    public BasePlayerController localPlayer;

    private Dictionary<int, BasePlayerController> players = new Dictionary<int, BasePlayerController>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(cinemachineCamera);
    }

    private void Start()
    {
        UIManager.Instance.OpenPanel<UIIngameMainPanel>();
        CreateCharacter(playerInRoom, Vector3.zero, Quaternion.identity);
    }

    public void CreateCharacter(GameObject prefab, Vector3 pos, Quaternion quaternion)
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate("Prefab/" + prefab.name, pos, quaternion);
            BasePlayerController playerController = playerInstance.GetComponent<BasePlayerController>();
            players.Add(PhotonNetwork.LocalPlayer.ActorNumber, playerController);
            photonView.RPC("AddPlayerInDictionary", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber, playerController.GetComponent<PhotonView>().ViewID);
        }
        else
        {
            playerInstance = Instantiate(prefab, pos, quaternion);
            players.Add(0, playerInstance.GetComponent<BasePlayerController>());
        }

        cinemachineCamera.Follow = playerInstance.transform;
        cinemachineCamera.LookAt = playerInstance.transform;

        //localPlayer = playerInstance.GetComponent<BasePlayerController>();
        //localPlayer.InitBlessing();
        //
        //
        //UIBase peekUI = UIManager.Instance.ReturnPeekUI();
        //if (peekUI is UIIngameMainPanel uIIngameMainPanel)
        //{
        //    localPlayer.updateUIAction += uIIngameMainPanel.UpdateUI;
        //    localPlayer.updateUIOutlineAction += uIIngameMainPanel.UpdateIconOutline;
        //}

    }

    [PunRPC]
    private void AddPlayerInDictionary(int actorNumber, int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            players.Add(actorNumber, targetView.GetComponent<BasePlayerController>());
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        foreach (var player in players)
        {
            int actorNumber = player.Key;
            int viewID = player.Value.GetComponent<PhotonView>().ViewID;
            photonView.RPC("AddPlayerInDictionary", newPlayer, actorNumber, viewID);
        }
    }


    public BasePlayerController ReturnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            return players[0];
        }
        else
        {
            return players[PhotonNetwork.LocalPlayer.ActorNumber];
        }
    }

    public UIConfirmPanel InteractWithDungeonNPC()
    {
        var panel = UIManager.Instance.OpenPopupPanel<UIConfirmPanel>();
        panel.Init(
            () => WaitForEnterStage(),
            () => UIManager.Instance.ClosePeekUI(),
            "게임 스테이지에 진입하시겠습니까?"
            );
        return panel;
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
