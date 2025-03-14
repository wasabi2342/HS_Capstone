using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerInRoom;
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    public BasePlayerController localPlayer;

    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

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
        //UIManager.Instance.OpenPanel<UIIngameMainPanel>();
        CreateCharacter(playerInRoom, Vector3.zero, Quaternion.Euler(90, 0, 0), true);
    }

    public void CreateCharacter(GameObject prefab, Vector3 pos, Quaternion quaternion, bool isInVillage)
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate("Prefab/" + prefab.name, pos, quaternion);
            players[PhotonNetwork.LocalPlayer.UserId] = playerInstance;
            PhotonNetworkManager.Instance.AddPlayer(PhotonNetwork.LocalPlayer.UserId, playerInstance.GetComponent<PhotonView>().ViewID);
            playerInstance.GetComponent<WhitePlayercontroller_event>().isInVillage = isInVillage; // 플레이어의 공통 스크립트로 변경 해야함

        }
        else
        {
            playerInstance = Instantiate(prefab, pos, quaternion);
            players["Local"] = playerInstance;
        }

        cinemachineCamera.Follow = playerInstance.transform;
        cinemachineCamera.LookAt = playerInstance.transform;
    }

    public GameObject ReturnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            return players["Local"];
        }
        else
        {
            return players[PhotonNetwork.LocalPlayer.UserId];
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
