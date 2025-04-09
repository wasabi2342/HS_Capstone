using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    [SerializeField]
    private GameObject defaultPlayer;
    [SerializeField]
    private CinemachineCamera playerCinemachineCamera;
    [SerializeField]
    private CinemachineCamera backgroundCinemachineCamera;
    [SerializeField]
    private bool isInVillage;
    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    // 카메라 전환에서 사용
    private List<GameObject> sortedPlayers;
    private int currentIndex = 0;

    public event Action<int, GameObject> UIUpdate;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        players = new Dictionary<int, GameObject>();
    }


    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "CharacterName"] != null)
        {
            Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "CharacterName"].ToString());
            CreateCharacter(PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "CharacterName"].ToString(), new Vector3(0, 2, 0), Quaternion.identity, isInVillage);
        }
        else
        {
            CreateCharacter(defaultPlayer.name, new Vector3(0, 2, 0), Quaternion.identity, isInVillage);
        }
    }

    public void CreateCharacter(string playerPrefabName, Vector3 pos, Quaternion quaternion, bool isInVillage)
    {
        GameObject playerInstance;

        if (PhotonNetwork.IsConnected)
        {
            playerInstance = PhotonNetwork.Instantiate("Prefab/" + playerPrefabName, pos, quaternion);
            players[PhotonNetwork.LocalPlayer.ActorNumber] = playerInstance;
            //PhotonNetworkManager.Instance.AddPlayer(PhotonNetwork.LocalPlayer.ActorNumber, playerInstance.GetComponent<PhotonView>().ViewID);
            playerInstance.GetComponent<Playercontroller_event>().isInVillage = isInVillage; // 플레이어의 공통 스크립트로 변경 해야함
            PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "CharacterName"] = playerPrefabName;
        }
        else
        {
            playerInstance = Instantiate(Resources.Load<ParentPlayerController>(playerPrefabName), pos, quaternion).gameObject;
            players[0] = playerInstance;
        }

        playerCinemachineCamera.Follow = playerInstance.transform;
        playerCinemachineCamera.LookAt = playerInstance.transform;
        backgroundCinemachineCamera.Follow = playerInstance.transform;
        backgroundCinemachineCamera.LookAt = playerInstance.transform;
    }

    public GameObject ReturnLocalPlayer()
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

    public void SwitchCameraToNextPlayer()
    {
        if (sortedPlayers.Count == 0) return;

        // 다음 플레이어로 이동 (마지막이면 0번으로 돌아감)
        currentIndex = (currentIndex + 1) % sortedPlayers.Count;

        // 카메라 변경
        playerCinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        playerCinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;
        backgroundCinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        backgroundCinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;
    }

    public void UpdateSortedPlayers()
    {
        sortedPlayers = players.OrderBy(p => p.Key == PhotonNetwork.LocalPlayer.ActorNumber ? 0 : 1)
                               .Select(p => p.Value)
                               .ToList();

        // 현재 따라가고 있는 플레이어가 리스트에서 몇 번째인지 찾기
        GameObject currentTarget = playerCinemachineCamera.Follow?.gameObject;
        int newIndex = sortedPlayers.FindIndex(p => p == currentTarget);

        // 유효한 값이면 currentIndex 갱신
        if (newIndex != -1)
            currentIndex = newIndex;
        else
            currentIndex = 0; // 기본적으로 첫 번째 플레이어를 바라보도록
    }

    public void AddPlayerDic(int actNum, GameObject player)
    {
        players[actNum] = player;
        UpdateSortedPlayers();
        UIUpdate?.Invoke(actNum, player);
    }
}
