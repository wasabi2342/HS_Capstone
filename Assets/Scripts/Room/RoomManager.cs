using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // ����������������������������������������������������������������
    // �ν����� ����
    // ����������������������������������������������������������������
    [SerializeField]
    private GameObject defaultPlayer;
    [SerializeField]
    private CinemachineCamera playerCinemachineCamera;
    [SerializeField]
    private CinemachineCamera backgroundCinemachineCamera;
    [SerializeField]
    private CinemachineCamera minimapCinemachineCamera;
    [SerializeField]
    private Camera UICamera;
    [SerializeField]
    private bool isInVillage;
    [SerializeField]
    private bool isInPvPArea = false;
    [SerializeField]
    private Vector3 playerScale = new Vector3(0.375f, 0.525f, 0.375f);
    [SerializeField]
    private List<Vector3> spawnPointList = new List<Vector3>();

    private Vector3 spawnPos;

    // ����������������������������������������������������������������
    // ��Ÿ�� ����
    // ����������������������������������������������������������������
    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    // ī�޶� ��ȯ���� ���
    private List<GameObject> sortedPlayers;
    private int currentIndex = 0;

    public event Action<int, GameObject> UIUpdate;

    // �±�, ���̾� ���
    private const string PLAYER_TAG = "Player";
    private int PLAYER_LAYER = 0;

    private System.Action<InputAction.CallbackContext> openMenuAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; // �̹� �ν��Ͻ��� ������ �ߺ� ���� ����
        }

        players = new Dictionary<int, GameObject>();
        int layerIdx = LayerMask.NameToLayer("Player");
        PLAYER_LAYER = layerIdx != -1 ? layerIdx : 0;

        // �� �ε� �� �÷��̾� ��� �ʱ�ȭ �� ����� ���� �߰�
        SceneManager.sceneLoaded += OnSceneLoadedClearPlayers;
    }

    private void OnDestroy() // Awake���� �߰��� �̺�Ʈ �ڵ鷯 ����
    {
        SceneManager.sceneLoaded -= OnSceneLoadedClearPlayers;
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && openMenuAction != null) // Null üũ �߰�
        {
            InputManager.Instance.PlayerInput.actions["OpenMenu"].performed -= openMenuAction;
        }
    }

    private void OnSceneLoadedClearPlayers(Scene scene, LoadSceneMode mode)
    {
        // �� ���� �ε�� ������ players ��ųʸ��� �ʱ�ȭ�մϴ�.
        Debug.Log($"Scene {scene.name} loaded. Clearing players dictionary.");
        players.Clear();
        isEnteringStage = false; // �� ��ȯ �� �������� ���� �÷��� �ʱ�ȭ
    }

    private void Start()
    {
        StartCoroutine(Co_Start());
    }

    IEnumerator Co_Start()
    {
        yield return new WaitForFixedUpdate();

        if (InputManager.Instance == null || InputManager.Instance.PlayerInput == null)
        {
            Debug.LogError("RoomManager.Co_Start: InputManager or PlayerInput not initialized!");
            yield break;
        }
        openMenuAction = OpenMenuPanel;
        InputManager.Instance.PlayerInput.actions["OpenMenu"].performed += openMenuAction;

        if (PhotonNetworkManager.Instance == null)
        {
            Debug.LogError("RoomManager.Co_Start: PhotonNetworkManager not initialized!");
            yield break;
        }
        PhotonNetworkManager.Instance.SetIsInPvPArea(isInPvPArea);

        // ���� �÷��̾��� ���� ĳ���� ��ü�� �ִٸ� �ı� (�� ��ȯ �� �ߺ� ����)
        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag(PLAYER_TAG);
            Debug.Log($"RoomManager.Co_Start: Found {existingPlayers.Length} existing objects with tag '{PLAYER_TAG}'.");
            foreach (GameObject oldPlayer in existingPlayers)
            {
                PhotonView pv = oldPlayer.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine && oldPlayer != null)
                {
                    bool isCurrentCharacter = false;
                    if (players.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out GameObject currentPlayerObject))
                    {
                        if (currentPlayerObject == oldPlayer)
                        {
                            isCurrentCharacter = true;
                        }
                    }

                    if (!isCurrentCharacter)
                    {
                        Debug.LogWarning($"RoomManager.Co_Start: Destroying potentially old player object for local player: {oldPlayer.name} (ViewID: {pv.ViewID})");
                        PhotonNetwork.Destroy(oldPlayer);
                    }
                }
            }
        }
        yield return null;

        string key = "SelectCharacter";

        List<Player> playerList = PhotonNetwork.PlayerList.ToList();
        int myIndex = playerList.IndexOf(PhotonNetwork.LocalPlayer);

        if (myIndex < 0 || myIndex >= spawnPointList.Count)
        {
            Debug.LogWarning("���� ����Ʈ�� ã�� �� �����ϴ�.");
            yield break;
        }

        spawnPos = spawnPointList[myIndex] + new Vector3(0, 1.5f, 0);

        if (PhotonNetwork.IsConnected
            && PhotonNetwork.CurrentRoom != null
            && PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(key, out var val)
            && val is string charName)
        {
            Debug.Log($"Loaded character name: {charName}");
            CreateCharacter(charName, spawnPos, Quaternion.identity, isInVillage);
        }
        else
        {
            CreateCharacter(defaultPlayer.name, spawnPos, Quaternion.identity, isInVillage);
        }

        if (UICamera != null)
        {
            UIManager.Instance.SetRenderCamera(UICamera);
        }
    }

    public void OpenMenuPanel(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.ReturnPeekUI() as UISkillInfoPanel)
            return;
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIMenuPanel>();
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
    }

    public void CreateCharacter(string playerPrefabName, Vector3 pos, Quaternion quaternion, bool isInVillage)
    {
        GameObject playerInstance;
        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (players.ContainsKey(localActorNumber) && players[localActorNumber] != null)
        {
            Debug.LogWarning($"RoomManager.CreateCharacter: Player for ActorNumber {localActorNumber} already exists. Skipping creation. Existing: {players[localActorNumber].name}");
            playerInstance = players[localActorNumber];
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log($"RoomManager.CreateCharacter: Instantiating new character '{playerPrefabName}' for local player (ActorNumber: {localActorNumber}).");
                playerInstance = PhotonNetwork.Instantiate("Prefab/" + playerPrefabName, pos, quaternion);
            }
            else
            {
                Debug.Log($"RoomManager.CreateCharacter: Instantiating new character '{playerPrefabName}' for offline mode.");
                playerInstance = Instantiate(Resources.Load<ParentPlayerController>(playerPrefabName), pos, quaternion).gameObject;
            }

            AddPlayerDic(PhotonNetwork.IsConnected ? localActorNumber : 0, playerInstance);

            if (PhotonNetwork.IsConnected)
                PhotonNetwork.CurrentRoom.CustomProperties[localActorNumber + "CharacterName"] = playerPrefabName;
        }

        playerInstance.transform.localScale = playerScale;
        playerInstance.GetComponent<Playercontroller_event>().isInVillage = isInVillage;
        playerInstance.GetComponent<ParentPlayerController>().SetIsInPVPArea(isInPvPArea);

        if (playerCinemachineCamera != null)
        {
            playerCinemachineCamera.Follow = playerInstance.transform;
            playerCinemachineCamera.LookAt = playerInstance.transform;
        }

        if (backgroundCinemachineCamera != null)
        {
            backgroundCinemachineCamera.Follow = playerInstance.transform;
            backgroundCinemachineCamera.LookAt = playerInstance.transform;
        }

        if (minimapCinemachineCamera != null)
        {
            minimapCinemachineCamera.Follow = playerInstance.transform;
            minimapCinemachineCamera.LookAt = playerInstance.transform;
        }

        UpdateSortedPlayers();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"RoomManager.OnPlayerEnteredRoom: Player {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber}) entered. Current player count in dictionary: {players.Count}");

        if (players.ContainsKey(newPlayer.ActorNumber) && players[newPlayer.ActorNumber] != null)
        {
            Debug.LogWarning($"RoomManager.OnPlayerEnteredRoom: Player {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber}) is already in the players dictionary. Current object: {players[newPlayer.ActorNumber].name}. Skipping LateCheckRemotePlayer.");
            return;
        }

        if (newPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            StartCoroutine(LateCheckRemotePlayer(newPlayer.ActorNumber));
        }
    }

    System.Collections.IEnumerator LateCheckRemotePlayer(int actorNum)
    {
        yield return new WaitForSeconds(1f);

        if (!players.ContainsKey(actorNum) || players[actorNum] == null)
        {
            Debug.Log($"RoomManager.LateCheckRemotePlayer: Player with ActorNumber {actorNum} not found or null in dictionary. Searching in scene...");
            foreach (var p in GameObject.FindGameObjectsWithTag(PLAYER_TAG))
            {
                if (p.TryGetComponent(out PhotonView pv) && pv.OwnerActorNr == actorNum)
                {
                    AddPlayerDic(actorNum, p);
                    break;
                }
            }
        }
    }

    public GameObject ReturnLocalPlayer() =>
        PhotonNetwork.InRoom ? players[PhotonNetwork.LocalPlayer.ActorNumber] : players[0];

    public UIConfirmPanel InteractWithDungeonNPC()
    {
        var panel = UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIConfirmPanel>();
        panel.Init(
            () => WaitForEnterStage(),
            () => UIManager.Instance.ClosePeekUI(),
            "���� ���������� �����Ͻðڽ��ϱ�?"
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

        currentIndex = (currentIndex + 1) % sortedPlayers.Count;

        playerCinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        playerCinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;

        backgroundCinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        backgroundCinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;

        minimapCinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        minimapCinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;
    }

    public void UpdateSortedPlayers()
    {
        if (isInVillage)
        {
            return;
        }
        sortedPlayers = players.OrderBy(p => p.Key == PhotonNetwork.LocalPlayer.ActorNumber ? 0 : 1)
                           .Select(p => p.Value)
                           .ToList();

        GameObject currentTarget = playerCinemachineCamera.Follow?.gameObject;
        int newIndex = sortedPlayers.FindIndex(p => p == currentTarget);

        if (newIndex != -1)
            currentIndex = newIndex;
        else
            currentIndex = 0;
    }

    public void AddPlayerDic(int actNum, GameObject player)
    {
        if (player == null)
        {
            Debug.LogError($"RoomManager.AddPlayerDic: Attempted to add a null player object for ActorNumber {actNum}.");
            return;
        }

        if (!players.ContainsKey(actNum) || players[actNum] == null)
        {
            players[actNum] = player;
            player.tag = PLAYER_TAG;
            SetLayerRecursively(player, PLAYER_LAYER);

            UpdateSortedPlayers();
            UIUpdate?.Invoke(actNum, player);
            Debug.Log($"RoomManager.AddPlayerDic: Player {actNum} (Object: {player.name}, ViewID: {player.GetComponent<PhotonView>()?.ViewID}) added to dictionary.");
        }
        else
        {
            if (players[actNum] != player)
            {
                Debug.LogWarning($"RoomManager.AddPlayerDic: ActorNumber {actNum} already exists in dictionary with a DIFFERENT object. Existing: {players[actNum].name} (ViewID: {players[actNum].GetComponent<PhotonView>()?.ViewID}), New: {player.name} (ViewID: {player.GetComponent<PhotonView>()?.ViewID}). This might indicate a duplicate.");
            }
            else
            {
                Debug.Log($"RoomManager.AddPlayerDic: Player {actNum} (Object: {player.name}) is already the same instance in dictionary. No action taken.");
            }
        }
    }

    // ����������������������������������������������������������������
    // ��ƿ
    // ����������������������������������������������������������������

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!PhotonNetwork.InRoom ||
            (PhotonNetwork.InRoom && other.GetComponentInParent<PhotonView>().IsMine)))
        {
            other.transform.position = spawnPos;
        }
    }
}