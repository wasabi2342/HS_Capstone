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
    // ────────────────────────────────
    // 인스펙터 참조
    // ────────────────────────────────
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

    // ────────────────────────────────
    // 런타임 변수
    // ────────────────────────────────
    public static RoomManager Instance { get; private set; }

    private Dictionary<int, bool> playerInRestrictedArea = new Dictionary<int, bool>();

    public bool isEnteringStage;

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    // 카메라 전환에서 사용
    private List<GameObject> sortedPlayers;
    private int currentIndex = 0;

    public event Action<int, GameObject> UIUpdate;

    // 태그, 레이어 상수
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
            return; // 이미 인스턴스가 있으면 중복 실행 방지
        }

        players = new Dictionary<int, GameObject>();
        int layerIdx = LayerMask.NameToLayer("Player");
        PLAYER_LAYER = layerIdx != -1 ? layerIdx : 0;

        // 씬 로드 시 플레이어 목록 초기화 및 재검증 로직 추가
        SceneManager.sceneLoaded += OnSceneLoadedClearPlayers;
    }

    private void OnDestroy() // Awake에서 추가한 이벤트 핸들러 제거
    {
        SceneManager.sceneLoaded -= OnSceneLoadedClearPlayers;
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && openMenuAction != null) // Null 체크 추가
        {
            InputManager.Instance.PlayerInput.actions["OpenMenu"].performed -= openMenuAction;
        }
    }

    private void OnSceneLoadedClearPlayers(Scene scene, LoadSceneMode mode)
    {
        // 새 씬이 로드될 때마다 players 딕셔너리를 초기화합니다.
        Debug.Log($"Scene {scene.name} loaded. Clearing players dictionary.");
        players.Clear();
        isEnteringStage = false; // 씬 전환 후 스테이지 진입 플래그 초기화
    }

    private void Start()
    {
        StartCoroutine(Co_Start());
    }

    IEnumerator Co_Start()
    {
        yield return new WaitForFixedUpdate(); // InputManager 등이 초기화될 시간을 줍니다.

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

        // 로컬 플레이어의 이전 캐릭터 객체가 있다면 파괴 (씬 전환 시 중복 방지)
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

        // 키 정의
        string key = "SelectCharacter";

        // 순서 기반 인덱스 구하기
        List<Player> playerList = PhotonNetwork.PlayerList.ToList(); // ActorNumber 순
        int myIndex = playerList.IndexOf(PhotonNetwork.LocalPlayer);

        // 인덱스 범위 확인
        if (myIndex < 0 || myIndex >= spawnPointList.Count)
        {
            Debug.LogWarning("스폰 포인트를 찾을 수 없습니다.");
            yield break; // 코루틴 종료
        }

        spawnPos = spawnPointList[myIndex] + new Vector3(0, 1.5f, 0);

        // 룸에 연결되어 있고 커스텀 프로퍼티에 해당 키가 있으면 안전하게 꺼내기
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

    // ────────────────────────────────
    // 유틸
    // ────────────────────────────────

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