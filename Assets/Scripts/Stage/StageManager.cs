using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    public static StageManager Instance;

    [Header("Prefabs (Resources 폴더에 있어야 함)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea 프리팹 이름
    public string doorPrefabName = "doorPrefab";
    public string blessingNPCPrefabName = "BlessingNPC"; // Blessing NPC 프리팹 이름

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // 현재 스테이지에 해당하는 설정
    public Quaternion spawnAreaRotation = Quaternion.identity; // 기본 회전 값

    private List<GameObject> spawnAreaInstances = new List<GameObject>();


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnStage();
            //PhotonNetwork.Instantiate(blessingNPCPrefabName, Vector3.zero, Quaternion.identity);
            PhotonNetwork.Instantiate(doorPrefabName, new Vector3(10,0,0), Quaternion.identity);

        }
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
            }

        }
        return cleared;
    }
    [PunRPC]
    public void RPC_OpenRewardUIForAll()
    {
        // 모든 클라이언트에서 UIManager를 통해 UIRewardPanel 프리팹을 Instantiate하여 보상 UI를 엽니다.
        UIManager.Instance.OpenPopupPanel<UIRewardPanel>();
    }

}
