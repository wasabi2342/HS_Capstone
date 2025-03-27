using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    [Header("Prefabs (Resources ������ �־�� ��)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea ������ �̸�

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // ���� ���������� �ش��ϴ� ����
    public Quaternion spawnAreaRotation = Quaternion.identity; // �⺻ ȸ�� ��

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

            // SpawnArea ������Ʈ�� �ݰ� ����
            SpawnArea area = spawnAreaInstance.GetComponent<SpawnArea>();
            if (area != null)
            {
                area.SetRadius(setting.radius);
                Debug.Log("Assigned radius: " + setting.radius + " to " + spawnAreaInstance.name);
            }
            else
            {
                Debug.LogWarning("SpawnArea ������Ʈ�� ã�� �� �����ϴ� in " + spawnAreaInstance.name);
            }

            // MonsterSpawner ������Ʈ�� �ڽĿ��� ã���� ����
            MonsterSpawner spawner = spawnAreaInstance.GetComponentInChildren<MonsterSpawner>();
            if (spawner != null)
            {
                spawner.SpawnMonsters(setting.monsterSpawnCount);
                Debug.Log("Spawned monsters using count: " + setting.monsterSpawnCount + " for " + spawnAreaInstance.name);
            }
            else
            {
                Debug.LogWarning("MonsterSpawner ������Ʈ�� ã�� �� �����ϴ� in " + spawnAreaInstance.name);
            }
        }
    }

    public bool AreAllMonstersCleared()
    {
        Debug.Log("ActiveMonsterCount: " + EnemyAI.ActiveMonsterCount);
        bool cleared = EnemyAI.ActiveMonsterCount == 0;
        if (cleared)
        {
            Debug.Log("��� ���Ͱ� ���ŵǾ����ϴ�.");
        }
        return cleared;
    }
}
