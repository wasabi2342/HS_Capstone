using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    [Header("Prefabs (Resources 폴더에 있어야 함)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea 프리팹 이름
    public string doorPrefabName = "doorPrefab";
    public string blessingNPCPrefabName = "BlessingNPC"; // Blessing NPC 프리팹 이름

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // 현재 스테이지에 해당하는 설정
    public Quaternion spawnAreaRotation = Quaternion.identity; // 기본 회전 값

    private List<GameObject> spawnAreaInstances = new List<GameObject>();

    private void Start()
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnStage();
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
                else
                {
                    Debug.LogError("Resources에서 doorPrefab을 찾을 수 없습니다: " + doorPrefabName);
                }
            }

        }
        return cleared;
    }
}
