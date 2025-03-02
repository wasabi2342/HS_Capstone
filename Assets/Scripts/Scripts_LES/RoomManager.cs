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

    public BasePlayerController localPlayer;

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
        CreateCharacter(playerInRoom, new Vector3(0, -0.35f, -0.35f), Quaternion.Euler(45, 0, 0));
    }

    public void CreateCharacter(GameObject prefab, Vector3 pos, Quaternion quaternion)  
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate(prefab.name, pos, quaternion);
        }
        else
        {
            playerInstance = Instantiate(prefab, pos, quaternion);
        }

        cinemachineCamera.Follow = playerInstance.transform;
        cinemachineCamera.LookAt = playerInstance.transform;
        
        localPlayer = playerInstance.GetComponent<BasePlayerController>();
        localPlayer.InitBlessing();
        
        
        UIBase peekUI = UIManager.Instance.ReturnPeekUI();
        if (peekUI is UIIngameMainPanel uIIngameMainPanel)
        {
            localPlayer.updateUIAction += uIIngameMainPanel.UpdateUI;
            localPlayer.updateUIOutlineAction += uIIngameMainPanel.UpdateIconOutline;
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
