using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    public static StageManager Instance;

    [Header("Prefabs (Resources ������ �־�� ��)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea ������ �̸�
    public string doorPrefabName = "doorPrefab";
    public string blessingNPCPrefabName = "BlessingNPC"; // Blessing NPC ������ �̸�

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // ���� ���������� �ش��ϴ� ����
    public Quaternion spawnAreaRotation = Quaternion.identity; // �⺻ ȸ�� ��

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
            // 1) ���� ���� ������ ����
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate(
                spawnAreaPrefabName,
                setting.position,
                spawnAreaRotation
            );
            spawnAreaInstances.Add(spawnAreaInstance);

            // 2) �ݰ� ����
            SpawnArea area = spawnAreaInstance.GetComponent<SpawnArea>();
            if (area != null)
            {
                area.SetRadius(setting.radius);
            }

            // 3) MonsterSpawner ã�Ƽ� ���� ����
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
            Debug.Log("��� ���Ͱ� ���ŵǾ����ϴ�.");
            if (PhotonNetwork.IsMasterClient)
            {
                // Resources �������� doorPrefab�� �ε�
                GameObject doorPrefab = Resources.Load<GameObject>(doorPrefabName);
                GameObject blessingNPC = Resources.Load<GameObject>(blessingNPCPrefabName);

                if (doorPrefab != null)
                {
                    // doorPrefab ���� ��ġ ���� (��: ù ��° SpawnArea�� ��ġ �Ǵ� StageManager ��ġ)
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
        // ��� Ŭ���̾�Ʈ���� UIManager�� ���� UIRewardPanel �������� Instantiate�Ͽ� ���� UI�� ���ϴ�.
        UIManager.Instance.OpenPopupPanel<UIRewardPanel>();
    }

}
