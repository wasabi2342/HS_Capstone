using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Prefabs (Resources 폴더에 있어야 함)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea 프리팹 이름
    public string doorPrefabName = "doorPrefab";
    public string blessingNPCPrefabName = "BlessingNPC"; // Blessing NPC 프리팹 이름
    public string CoopOrBetrayNPCPrefabName = "CoopOrBetrayNPC"; // coopOrBetrayNPCPos 프리팹 이름

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // 현재 스테이지에 해당하는 설정
    public Quaternion spawnAreaRotation = Quaternion.identity; // 기본 회전 값

    private List<GameObject> spawnAreaInstances = new List<GameObject>();

    [SerializeField]
    private Transform coopOrBetrayNPCPos;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        // 브금 종료
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCurrentBGM();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnStage();
            //PhotonNetwork.Instantiate(blessingNPCPrefabName, Vector3.zero, Quaternion.identity);
            PhotonNetwork.Instantiate(doorPrefabName, new Vector3(10,0,0), Quaternion.identity);
            PhotonNetwork.Instantiate(CoopOrBetrayNPCPrefabName, coopOrBetrayNPCPos.position, Quaternion.identity);

        }
        // RoomProperties에서 인덱스 읽기
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("FinalRewardIndex"))
        {
            int chosenIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["FinalRewardIndex"];
            Debug.Log($"이 방에서 확정된 보상 인덱스: {chosenIndex}");

            // (원하는 아이템 지급 / 능력치 상승 / 골드 부여 등)
            ApplyReward(chosenIndex);
        }
        else
        {
            Debug.LogWarning("FinalRewardIndex가 존재하지 않습니다!");
        }
    }
    private void ApplyReward(int rewardIndex)
    {
        // 실제 프로젝트 로직에 맞춰 구현
    }
    void SpawnStage()
    {
        foreach (SpawnAreaSetting setting in currentStageSettings.spawnAreaSettings)
        {
            // 1) 스폰 영역 프리팹 생성
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate(
                spawnAreaPrefabName,
                setting.position,
                spawnAreaRotation
            );
            spawnAreaInstances.Add(spawnAreaInstance);

            // 2) 반경 설정
            SpawnArea area = spawnAreaInstance.GetComponent<SpawnArea>();
            if (area != null)
            {
                area.SetRadius(setting.radius);
            }

            // 3) MonsterSpawner 찾아서 몬스터 스폰
            MonsterSpawner spawner = spawnAreaInstance.GetComponentInChildren<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.SpawnMonsters(setting.monsterSpawnInfos);
            }
        }
    }

    public bool AreAllMonstersCleared()
    {
        Debug.Log("ActiveMonsterCount: " + EnemyAI.ActiveMonsterCount);
        bool cleared = EnemyAI.ActiveMonsterCount == 0;
        if (cleared)
        {
            Debug.Log("모든 몬스터가 제거되었습니다.");
            if (PhotonNetwork.IsMasterClient)
            {
                // Resources 폴더에서 doorPrefab을 로드
                GameObject doorPrefab = Resources.Load<GameObject>(doorPrefabName);
                GameObject blessingNPC = Resources.Load<GameObject>(blessingNPCPrefabName);

                if (doorPrefab != null)
                {
                    // doorPrefab 생성 위치 설정 (예: 첫 번째 SpawnArea의 위치 또는 StageManager 위치)
                    Vector3 doorSpawnPosition = spawnAreaInstances.Count > 0 ? spawnAreaInstances[0].transform.position : transform.position;
                    PhotonNetwork.Instantiate(doorPrefabName, doorSpawnPosition, Quaternion.identity);
                }
                if (blessingNPC != null)
                {
                    Vector3 blessingNPCSpawnPosition = spawnAreaInstances.Count > 1 ? spawnAreaInstances[1].transform.position : transform.position;
                    PhotonNetwork.Instantiate(blessingNPCPrefabName, blessingNPCSpawnPosition, Quaternion.identity);
                }
                if(MonsterStatusManager.instance != null)
                {
                    MonsterStatusManager.instance.ResetEnemyDamage();
                }
            }

        }
        return cleared;
    }

}
