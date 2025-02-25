using Photon.Pun;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class RoomManager : MonoBehaviourPun
{
    [SerializeField]
    private GameObject playerInRoom;
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate(playerInRoom.name, new Vector3(0, -0.35f, -0.35f), Quaternion.Euler(45, 0, 0));
        }
        else
        {
            playerInstance = Instantiate(playerInRoom, new Vector3(0, -0.35f, -0.35f), Quaternion.Euler(45, 0, 0));
        }
        cinemachineCamera.Follow = playerInstance.transform;
        cinemachineCamera.LookAt = playerInstance.transform;

        isEnteringStage = false;
    }

    public void CreateCharacter(GameObject prefab, Transform transform)  
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate(prefab.name, transform.position, transform.rotation);
        }
        else
        {
            playerInstance = Instantiate(prefab, transform.position, transform.rotation);
        }
        cinemachineCamera.Follow = playerInstance.transform;
        cinemachineCamera.LookAt = playerInstance.transform;
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
