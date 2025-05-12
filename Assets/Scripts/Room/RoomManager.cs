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

    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환시 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        players = new Dictionary<int, GameObject>();
        int layerIdx = LayerMask.NameToLayer("Player");
        PLAYER_LAYER = layerIdx != -1 ? layerIdx : 0;

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoadedClearPlayers;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoadedClearPlayers;
    }

    private void OnSceneLoadedClearPlayers(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 {scene.name} 로드됨. 플레이어 정리 시작");
        
        // 기존 플레이어 객체 정리
        foreach (var player in players.Values.ToList())
        {
            if (player != null)
            {
                Debug.Log($"기존 플레이어 제거: {player.name}");
                PhotonNetwork.Destroy(player);
            }
        }
        
        players.Clear();
        isEnteringStage = false;
        isInitialized = false;  // 초기화 상태 리셋

        // 씬의 카메라 참조 찾기
        FindCameraReferences();
        
        // 씬 전환 후 즉시 플레이어 생성 시작
        StartCoroutine(Co_Start());
    }

    private void FindCameraReferences()
    {
        // 씬에서 카메라 찾기
        var cinemachineCameras = FindObjectsOfType<CinemachineCamera>();
        foreach (var cam in cinemachineCameras)
        {
            if (cam.name.Contains("Player"))
                playerCinemachineCamera = cam;
            else if (cam.name.Contains("Background"))
                backgroundCinemachineCamera = cam;
            else if (cam.name.Contains("Minimap"))
                minimapCinemachineCamera = cam;
        }

        // UI 카메라 찾기
        UICamera = FindObjectOfType<Camera>();
        
        Debug.Log($"카메라 참조 업데이트: Player={playerCinemachineCamera != null}, " +
                  $"Background={backgroundCinemachineCamera != null}, " +
                  $"Minimap={minimapCinemachineCamera != null}, " +
                  $"UI={UICamera != null}");
    }

    private void Start()
    {
        StartCoroutine(Co_Start());
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerInput.actions["OpenMenu"].performed -= openMenuAction;
    }

    IEnumerator Co_Start()
    {
        yield return new WaitForFixedUpdate();

        // 이미 초기화되었다면 중복 실행 방지
        if (isInitialized)
        {
            Debug.Log("이미 초기화되어 있습니다.");
            yield break;
        }

        Debug.Log("플레이어 초기화 시작");

        // 입력 시스템 초기화
        if (InputManager.Instance != null)
        {
            openMenuAction = OpenMenuPanel;
            InputManager.Instance.PlayerInput.actions["OpenMenu"].performed += openMenuAction;
            Debug.Log("입력 시스템 초기화 완료");
        }
        else
        {
            Debug.LogError("InputManager를 찾을 수 없습니다!");
        }

        PhotonNetworkManager.Instance.SetIsInPvPArea(isInPvPArea);

        // 키 정의
        string key = "SelectCharacter";

        // 순서 기반 인덱스 구하기
        List<Player> playerList = PhotonNetwork.PlayerList.ToList(); // ActorNumber 순
        int myIndex = playerList.IndexOf(PhotonNetwork.LocalPlayer);

        // 인덱스 범위 확인
        if (myIndex < 0 || myIndex >= spawnPointList.Count)
        {
            Debug.LogWarning("스폰 포인트를 찾을 수 없습니다.");
            yield break;
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

        // UI 카메라 설정
        if (UICamera != null && UIManager.Instance != null)
        {
            UIManager.Instance.SetRenderCamera(UICamera);
            Debug.Log("UI 카메라 설정 완료");
        }
        else
        {
            Debug.LogWarning("UI 카메라 또는 UIManager를 찾을 수 없습니다!");
        }

        isInitialized = true;
        Debug.Log("플레이어 초기화 완료");
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
        Debug.Log($"플레이어 생성 시작: {playerPrefabName}");

        // 이미 존재하는 플레이어 객체가 있다면 제거
        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        if (players.ContainsKey(localActorNumber) && players[localActorNumber] != null)
        {
            Debug.Log($"기존 플레이어 객체 제거: ActorNumber {localActorNumber}");
            PhotonNetwork.Destroy(players[localActorNumber]);
            players.Remove(localActorNumber);
        }

        GameObject playerInstance;
        if (PhotonNetwork.IsConnected)
        {
            // 마스터 클라이언트가 아닌 경우에만 새 플레이어 생성
            if (!PhotonNetwork.IsMasterClient || localActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                Debug.Log($"플레이어 인스턴스 생성: {playerPrefabName}");
                playerInstance = PhotonNetwork.Instantiate("Prefab/" + playerPrefabName, pos, quaternion);
                players[localActorNumber] = playerInstance;
                PhotonNetwork.CurrentRoom.CustomProperties[localActorNumber + "CharacterName"] = playerPrefabName;
            }
            else
            {
                Debug.Log($"마스터 클라이언트: 다른 플레이어 {localActorNumber}의 생성은 건너뜁니다");
                return;
            }
        }
        else
        {
            playerInstance = Instantiate(Resources.Load<ParentPlayerController>(playerPrefabName), pos, quaternion).gameObject;
            players[0] = playerInstance;
        }

        // 플레이어 설정
        if (playerInstance != null)
        {
            Debug.Log($"플레이어 설정 시작: {playerInstance.name}");
            playerInstance.tag = PLAYER_TAG;
            SetLayerRecursively(playerInstance, PLAYER_LAYER);
            playerInstance.transform.localScale = playerScale;
            
            // 컴포넌트 설정
            var playerEvent = playerInstance.GetComponent<Playercontroller_event>();
            if (playerEvent != null)
            {
                playerEvent.isInVillage = isInVillage;
            }
            else
            {
                Debug.LogError($"Playercontroller_event 컴포넌트를 찾을 수 없습니다: {playerInstance.name}");
            }

            var playerController = playerInstance.GetComponent<ParentPlayerController>();
            if (playerController != null)
            {
                playerController.SetIsInPVPArea(isInPvPArea);
            }
            else
            {
                Debug.LogError($"ParentPlayerController 컴포넌트를 찾을 수 없습니다: {playerInstance.name}");
            }

            // 카메라 설정
            if (playerCinemachineCamera != null)
            {
                playerCinemachineCamera.Follow = playerInstance.transform;
                playerCinemachineCamera.LookAt = playerInstance.transform;
                Debug.Log("플레이어 카메라 설정 완료");
            }

            if (backgroundCinemachineCamera != null)
            {
                backgroundCinemachineCamera.Follow = playerInstance.transform;
                backgroundCinemachineCamera.LookAt = playerInstance.transform;
                Debug.Log("배경 카메라 설정 완료");
            }

            if (minimapCinemachineCamera != null)
            {
                minimapCinemachineCamera.Follow = playerInstance.transform;
                minimapCinemachineCamera.LookAt = playerInstance.transform;
                Debug.Log("미니맵 카메라 설정 완료");
            }

            UpdateSortedPlayers();
            Debug.Log($"플레이어 설정 완료: {playerInstance.name}");
        }
        else
        {
            Debug.LogError($"플레이어 인스턴스 생성 실패: {playerPrefabName}");
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // 네트워크로 전송되는 Instantiate 시점에 AddPlayerDic() 을 RPC 로 호출해두면 됨
        // 단, 여기에서도 혹시 누락된 원격 플레이어가 있으면 찾아서 사후 처리
        StartCoroutine(LateCheckRemotePlayer(newPlayer.ActorNumber));
    }

    System.Collections.IEnumerator LateCheckRemotePlayer(int actorNum)
    {
        yield return new WaitForSeconds(1f);   // 네트워크 Instantiate 완료 대기

        if (!players.ContainsKey(actorNum))
        {
            // 씬 내 모든 Player 태그 검색 후 PhotonView 체크
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
    /*
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
    */

    public GameObject ReturnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom)
        {
            if (players.TryGetValue(0, out GameObject offlinePlayer))
            {
                return offlinePlayer;
            }
            Debug.LogWarning("오프라인 모드에서 플레이어를 찾을 수 없습니다.");
            return null;
        }
        else
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            if (players.TryGetValue(actorNumber, out GameObject onlinePlayer))
            {
                return onlinePlayer;
            }
            Debug.LogWarning($"온라인 모드에서 플레이어를 찾을 수 없습니다. ActorNumber: {actorNumber}");
            return null;
        }
    }

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

        // 다음 플레이어로 이동 (마지막이면 0번으로 돌아감)
        currentIndex = (currentIndex + 1) % sortedPlayers.Count;

        // 카메라 변경
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
        if (!players.ContainsKey(actNum))
        {
            players[actNum] = player;
            player.tag = PLAYER_TAG;
            SetLayerRecursively(player, PLAYER_LAYER);

            UpdateSortedPlayers();
            UIUpdate?.Invoke(actNum, player);
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