using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

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

    // ī�޶� ��ȯ���� ���
    private List<GameObject> sortedPlayers;
    private int currentIndex = 0;

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
        CreateCharacter(playerInRoom, new Vector3(0, 2, 0), Quaternion.identity, true);
    }

    public void CreateCharacter(GameObject prefab, Vector3 pos, Quaternion quaternion, bool isInVillage)
    {
        GameObject playerInstance;
        if (PhotonNetwork.InRoom)
        {
            playerInstance = PhotonNetwork.Instantiate("Prefab/" + prefab.name, pos, quaternion);
            players[PhotonNetwork.LocalPlayer.UserId] = playerInstance;
            PhotonNetworkManager.Instance.AddPlayer(PhotonNetwork.LocalPlayer.UserId, playerInstance.GetComponent<PhotonView>().ViewID);
            playerInstance.GetComponent<WhitePlayercontroller_event>().isInVillage = isInVillage; // �÷��̾��� ���� ��ũ��Ʈ�� ���� �ؾ���

        }
        else
        {
            playerInstance = Instantiate(prefab, pos, quaternion);
            players["Local"] = playerInstance;
        }

        DontDestroyOnLoad(playerInstance);
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

        // ���� �÷��̾�� �̵� (�������̸� 0������ ���ư�)
        currentIndex = (currentIndex + 1) % sortedPlayers.Count;

        // ī�޶� ����
        cinemachineCamera.Follow = sortedPlayers[currentIndex].transform;
        cinemachineCamera.LookAt = sortedPlayers[currentIndex].transform;
    }

    public void UpdateSortedPlayers()
    {
        sortedPlayers = players.OrderBy(p => p.Key == PhotonNetwork.LocalPlayer.UserId ? 0 : 1)
                               .Select(p => p.Value)
                               .ToList();

        // ���� ���󰡰� �ִ� �÷��̾ ����Ʈ���� �� ��°���� ã��
        GameObject currentTarget = cinemachineCamera.Follow?.gameObject;
        int newIndex = sortedPlayers.FindIndex(p => p == currentTarget);

        // ��ȿ�� ���̸� currentIndex ����
        if (newIndex != -1)
            currentIndex = newIndex;
        else
            currentIndex = 0; // �⺻������ ù ��° �÷��̾ �ٶ󺸵���
    }

}
