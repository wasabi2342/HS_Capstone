using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    [Header("Prefabs (Resources 폴더에 있어야 함)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea 프리팹 이름

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
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate(spawnAreaPrefabName, setting.position, spawnAreaRotation);
            spawnAreaInstances.Add(spawnAreaInstance);
            Debug.Log("Spawned SpawnArea: " + spawnAreaInstance.name + " at position: " + setting.position);

            // SpawnArea 컴포넌트에 반경 설정
            SpawnArea area = spawnAreaInstance.GetComponent<SpawnArea>();
            if (area != null)
            {
                area.SetRadius(setting.radius);
                Debug.Log("Assigned radius: " + setting.radius + " to " + spawnAreaInstance.name);
            }
            else
            {
                Debug.LogWarning("SpawnArea 컴포넌트를 찾을 수 없습니다 in " + spawnAreaInstance.name);
            }

            // MonsterSpawner 컴포넌트를 자식에서 찾도록 수정
            MonsterSpawner spawner = spawnAreaInstance.GetComponentInChildren<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.SpawnMonsters(setting.monsterSpawnCount);
                Debug.Log("Spawned monsters using count: " + setting.monsterSpawnCount + " for " + spawnAreaInstance.name);
            }
            else
            {
                Debug.LogWarning("MonsterSpawner 컴포넌트를 찾을 수 없습니다 in " + spawnAreaInstance.name);
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
        }
        return cleared;
    }
}
