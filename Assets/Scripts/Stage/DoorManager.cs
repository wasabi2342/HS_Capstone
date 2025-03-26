using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class DoorManager : MonoBehaviourPun
{
    // DoorSettings 에셋 (문 프리팹, 문 스폰 설정, 대기 시간 등)
    public DoorSettings doorSettings;
    // StageManager 참조 (Inspector에서 할당)
    public StageManager stageManager;

    private List<Door> doors = new List<Door>();
    private int totalPlayers => PhotonNetwork.PlayerList.Length;

    private void Awake()
    {
        Debug.Log("DoorManager Awake: MasterClient = " + PhotonNetwork.IsMasterClient);
    }

    private void Start()
    {
        Debug.Log("DoorManager Start called.");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("MasterClient detected. Starting CheckAndTransition coroutine.");
            StartCoroutine(CheckAndTransition());
        }
    }

    public void SpawnDoors()
    {
        if (doorSettings.doorPrefab == null)
        {
            Debug.LogError("DoorSettings.doorPrefab is not assigned!");
            return;
        }
        Debug.Log("SpawnDoors() called - spawning doors.");
        foreach (DoorSpawnSetting setting in doorSettings.doorSpawnSettings)
        {
            GameObject doorObj = PhotonNetwork.Instantiate(doorSettings.doorPrefab.name, setting.position, setting.rotation);
            Door door = doorObj.GetComponent<Door>();
            if (door != null)
            {
                // 설정에서 보상 타입 할당
                door.rewardType = setting.rewardType;
                doors.Add(door);
                Debug.Log($"Spawned Door: {doorObj.name} at {setting.position}, RewardType: {setting.rewardType}");
            }
            else
            {
                Debug.LogWarning("Door component not found on spawned door object: " + doorObj.name);
            }
        }
    }

    bool AllPlayersInteracted()
    {
        int totalInteractions = 0;
        foreach (var door in doors)
        {
            totalInteractions += door.interactionCount;
        }
        Debug.Log($"Total interactions: {totalInteractions} / {totalPlayers}");
        return totalInteractions >= totalPlayers;
    }

    public IEnumerator CheckAndTransition()
    {
        Debug.Log("CheckAndTransition coroutine started.");

        // 몬스터 제거 대기 단계
        while (!stageManager.AreAllMonstersCleared())
        {
            Debug.Log("Waiting for monsters to be cleared...");
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("All monsters cleared.");

        // 문이 아직 생성되지 않았다면 생성
        if (doors.Count == 0)
        {
            Debug.Log("No doors found. Spawning doors.");
            SpawnDoors();
        }

        // 모든 플레이어가 문과 상호작용할 때까지 대기
        while (!AllPlayersInteracted())
        {
            Debug.Log("Waiting for all players to interact with doors...");
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("All players have interacted with doors.");

        // 추가 대기 시간 (5초, doorSettings.waitTime)
        yield return new WaitForSeconds(doorSettings.waitTime);
        Debug.Log($"Wait time ({doorSettings.waitTime} sec) elapsed. Calculating door selection...");

        // 확률 계산: 각 문의 상호작용 횟수를 바탕으로 확률 부여
        float randomValue = Random.value;
        Debug.Log("Random value: " + randomValue);
        float cumulative = 0f;
        Door selectedDoor = null;
        foreach (var door in doors)
        {
            float probability = (float)door.interactionCount / totalPlayers;
            cumulative += probability;
            Debug.Log($"{door.name} probability: {probability}, cumulative: {cumulative}");
            if (randomValue <= cumulative)
            {
                selectedDoor = door;
                break;
            }
        }

        if (selectedDoor != null)
        {
            Debug.Log($"Selected door: {selectedDoor.name} at {selectedDoor.transform.position}, RewardType: {selectedDoor.rewardType}");
            // 전역 보상 데이터에 선택된 보상 타입 저장
            RewardData.SelectedRewardType = selectedDoor.rewardType;
            photonView.RPC("RPC_TransitionStage", RpcTarget.All, selectedDoor.photonView.ViewID);
        }
        else
        {
            Debug.LogError("Door selection failed. Check probability calculation.");
        }
    }

    [PunRPC]
    void RPC_TransitionStage(int doorViewID)
    {
        Door door = PhotonView.Find(doorViewID).GetComponent<Door>();
        if (door != null)
        {
            Debug.Log($"Transitioning through door: {door.name}, RewardType: {door.rewardType}");
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel("StageTest2");
            }
        }
        else
        {
            Debug.LogError("RPC_TransitionStage: Door not found for viewID " + doorViewID);
        }
    }
}
