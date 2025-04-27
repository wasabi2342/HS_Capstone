using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Prefabs (Resources ������ �־�� ��)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea ������ �̸�
    public string doorPrefabName = "doorPrefab";
    public string blessingNPCPrefabName = "BlessingNPC"; // Blessing NPC ������ �̸�
    public string CoopOrBetrayNPCPrefabName = "CoopOrBetrayNPC"; // coopOrBetrayNPCPos ������ �̸�

    [Header("Stage Settings")]
    public StageSettings currentStageSettings; // ���� ���������� �ش��ϴ� ����
    public Quaternion spawnAreaRotation = Quaternion.identity; // �⺻ ȸ�� ��

    private List<GameObject> spawnAreaInstances = new List<GameObject>();

    [SerializeField]
    private Transform coopOrBetrayNPCPos;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        // ��� ����
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
        // RoomProperties���� �ε��� �б�
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("FinalRewardIndex"))
        {
            int chosenIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["FinalRewardIndex"];
            Debug.Log($"�� �濡�� Ȯ���� ���� �ε���: {chosenIndex}");

            // (���ϴ� ������ ���� / �ɷ�ġ ��� / ��� �ο� ��)
            ApplyReward(chosenIndex);
        }
        else
        {
            Debug.LogWarning("FinalRewardIndex�� �������� �ʽ��ϴ�!");
        }
    }
    private void ApplyReward(int rewardIndex)
    {
        // ���� ������Ʈ ������ ���� ����
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
                if(MonsterStatusManager.instance != null)
                {
                    MonsterStatusManager.instance.ResetEnemyDamage();
                }
            }

        }
        return cleared;
    }

}
