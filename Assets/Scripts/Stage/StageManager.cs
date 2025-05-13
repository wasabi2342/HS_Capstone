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
    private List<GameObject> spawnAreaInstances = new List<GameObject>();
    private Quaternion spawnAreaRotation = Quaternion.Euler(0, 0, 0); // �⺻ ȸ���� (0��)
    private int currentWave = 0;

    [SerializeField]
    private Transform coopOrBetrayNPCPos;
    public Transform rewardSpawn;
    public Transform blessingSpawn;

    [SerializeField]
    private List<string> CoopOrBetrayNPCPrefabNameList = new List<string>();
    [SerializeField]
    private List<Transform> coopOrBetrayNPCPosList = new List<Transform>();
    [SerializeField]
    private List<bool> isImmediateSpawnList = new List<bool>();

    [SerializeField]
    private bool isEndStage = false;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnWave(0);
            for(int i = 0; i < isImmediateSpawnList.Count; i++)
            {
                if (isImmediateSpawnList[i])
                {
                    GameObject coopOrBetrayNPC = Resources.Load<GameObject>(CoopOrBetrayNPCPrefabNameList[i]);
                    if (coopOrBetrayNPC != null)
                    {
                        PhotonNetwork.Instantiate(
                        CoopOrBetrayNPCPrefabNameList[i],
                        coopOrBetrayNPCPosList[i].position,
                        coopOrBetrayNPCPosList[i].rotation);
                    }
                }
            }
            //PhotonNetwork.Instantiate(blessingNPCPrefabName, Vector3.zero, Quaternion.identity);
            //PhotonNetwork.Instantiate(doorPrefabName, new Vector3(10,0,0), Quaternion.identity);
            //if (coopOrBetrayNPCPos != null)
            //{
            //    PhotonNetwork.Instantiate(CoopOrBetrayNPCPrefabName, coopOrBetrayNPCPos.position, Quaternion.identity);
            //}

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
    void SpawnWave(int waveIdx)
    {
        currentWave = waveIdx;

        // --- ù ���̺�(0)�� �� SpawnArea ������ ���� ---
        if (waveIdx == 0)
        {
            foreach (var setting in currentStageSettings.waves[0].spawnAreaSettings)
            {
                var areaGO = PhotonNetwork.Instantiate(
                    spawnAreaPrefabName, setting.position, spawnAreaRotation);
                spawnAreaInstances.Add(areaGO);

                // �ݰ� ����
                var area = areaGO.GetComponent<SpawnArea>();
                area.SetRadius(setting.radius);
            }
        }

        // --- �ش� ���̺� ���͸� ���� ---
        for (int i = 0; i < spawnAreaInstances.Count; i++)
        {
            var spawner = spawnAreaInstances[i]
                .GetComponentInChildren<MonsterSpawner>();
            if (spawner != null)
            {
                var infos = currentStageSettings
                    .waves[waveIdx].spawnAreaSettings[i]
                    .monsterSpawnInfos;
                spawner.SpawnMonsters(infos);
            }
        }
    }
    public void OnAllMonsterCleared()
    {
        if (!PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ�� ����
        if (currentWave < currentStageSettings.waves.Length - 1)
        {
            SpawnWave(currentWave + 1); // ���� ���̺� ����
        }
        else
        {
            AreAllMonstersCleared(); // ��� ���� ���� Ȯ��
        }
    }

    public bool AreAllMonstersCleared()
    {
        Debug.Log("ActiveMonsterCount: " + EnemyFSM.ActiveMonsterCount);
        bool cleared = EnemyFSM.ActiveMonsterCount == 0;
        if (cleared)
        {
            Debug.Log("��� ���Ͱ� ���ŵǾ����ϴ�.");
            if (PhotonNetwork.IsMasterClient)
            {
                if (isEndStage && PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    PhotonNetworkManager.Instance.EndGameInSoloPlay();
                }
                // Resources �������� doorPrefab�� �ε�
                GameObject doorPrefab = Resources.Load<GameObject>(doorPrefabName);
                GameObject blessingNPC = Resources.Load<GameObject>(blessingNPCPrefabName);

                if (doorPrefab != null)
                {
                    PhotonNetwork.Instantiate(
                    doorPrefabName,
                    rewardSpawn.position,
                    rewardSpawn.rotation);
                }
                if (blessingNPC != null)
                {
                    PhotonNetwork.Instantiate(
                    blessingNPCPrefabName,
                    blessingSpawn.position,
                    blessingSpawn.rotation);
                }
                for (int i = 0; i < isImmediateSpawnList.Count; i++)
                {
                    if (isImmediateSpawnList[i])
                    {
                        GameObject coopOrBetrayNPC = Resources.Load<GameObject>(CoopOrBetrayNPCPrefabNameList[i]);
                        if (coopOrBetrayNPC != null)
                        {
                            PhotonNetwork.Instantiate(
                            CoopOrBetrayNPCPrefabNameList[i],
                            coopOrBetrayNPCPosList[i].position,
                            coopOrBetrayNPCPosList[i].rotation);
                        }
                    }
                }
                if (MonsterStatusManager.instance != null)
                {
                    MonsterStatusManager.instance.ResetEnemyDamage();
                }
            }

        }
        return cleared;
    }

}
