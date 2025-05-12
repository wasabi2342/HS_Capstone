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
        yield return new WaitForFixedUpdate(); // InputManager ���� �ʱ�ȭ�� �ð��� �ݴϴ�.

        if (InputManager.Instance == null || InputManager.Instance.PlayerInput == null)
        {
            Debug.LogError("InputManager or PlayerInput not initialized!");
            yield break;
        }
        openMenuAction = OpenMenuPanel;
        InputManager.Instance.PlayerInput.actions["OpenMenu"].performed += openMenuAction;

        if (PhotonNetworkManager.Instance == null)
        {
            Debug.LogError("PhotonNetworkManager not initialized!");
            yield break;
        }
        PhotonNetworkManager.Instance.SetIsInPvPArea(isInPvPArea);

        // ���� �÷��̾��� ���� ĳ���� ��ü�� �ִٸ� �ı� (�� ��ȯ �� �ߺ� ����)
        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag(PLAYER_TAG);
            foreach (GameObject oldPlayer in existingPlayers)
            {
                PhotonView pv = oldPlayer.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    if (!players.ContainsValue(oldPlayer))
                    {
                        Debug.LogWarning($"Destroying old player object for local player: {oldPlayer.name}");
                        PhotonNetwork.Destroy(oldPlayer);
                    }
                }
            }
        }

        // Ű ����
        string key = "SelectCharacter";

        // ���� ��� �ε��� ���ϱ�
        List<Player> playerList = PhotonNetwork.PlayerList.ToList(); // ActorNumber ��
        int myIndex = playerList.IndexOf(PhotonNetwork.LocalPlayer);

        // �ε��� ���� Ȯ��
        if (myIndex < 0 || myIndex >= spawnPointList.Count)
        {
            Debug.LogWarning("���� ����Ʈ�� ã�� �� �����ϴ�.");
            yield break; // �ڷ�ƾ ����
        }

        spawnPos = spawnPointList[myIndex] + new Vector3(0, 1.5f, 0);

        // �뿡 ����Ǿ� �ְ� Ŀ���� ������Ƽ�� �ش� Ű�� ������ �����ϰ� ������
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

        if (PhotonNetwork.IsConnected)
        {
            playerInstance = PhotonNetwork.Instantiate("Prefab/" + playerPrefabName, pos, quaternion);
            players[PhotonNetwork.LocalPlayer.ActorNumber] = playerInstance;
            PhotonNetwork.CurrentRoom.CustomProperties[PhotonNetwork.LocalPlayer.ActorNumber + "CharacterName"] = playerPrefabName;
        }
        else
        {
            playerInstance = Instantiate(Resources.Load<ParentPlayerController>(playerPrefabName), pos, quaternion).gameObject;
            players[0] = playerInstance;
        }
        playerInstance.tag = PLAYER_TAG;
        SetLayerRecursively(playerInstance, PLAYER_LAYER);
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
        Debug.Log($"OnPlayerEnteredRoom: {newPlayer.NickName}. Current player count in dictionary: {players.Count}");

        if (players.ContainsKey(newPlayer.ActorNumber))
        {
            Debug.LogWarning($"Player {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber}) is already in the players dictionary. Skipping LateCheckRemotePlayer.");
            return;
        }

        StartCoroutine(LateCheckRemotePlayer(newPlayer.ActorNumber));
    }

    System.Collections.IEnumerator LateCheckRemotePlayer(int actorNum)
    {
        yield return new WaitForSeconds(1f);

        if (!players.ContainsKey(actorNum))
        {
            Debug.Log($"LateCheckRemotePlayer: Player with ActorNumber {actorNum} not found in dictionary. Searching in scene...");
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
        if (!players.ContainsKey(actNum))
        {
            players[actNum] = player;
            player.tag = PLAYER_TAG;
            SetLayerRecursively(player, PLAYER_LAYER);

            UpdateSortedPlayers();
            UIUpdate?.Invoke(actNum, player);
            Debug.Log($"Player {actNum} added to dictionary. Player object: {player.name}");
        }
        else
        {
            Debug.LogWarning($"AddPlayerDic: Player with ActorNumber {actNum} already exists in dictionary. Player object: {player.name}, Existing object: {players[actNum].name}");
            if (players[actNum] != player && players[actNum] != null)
            {
                Debug.LogWarning($"Duplicate player object detected for ActorNumber {actNum}. Destroying old object: {players[actNum].name}");
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