using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

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
            EnemyFSM.ActiveMonsterCount = 0;

            SpawnWave(0);
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
            if (isEndStage)
            {
                GameObject blessingNPC = Resources.Load<GameObject>(blessingNPCPrefabName);

                if (blessingNPC != null)
                {
                    PhotonNetwork.Instantiate(
                    blessingNPCPrefabName,
                    blessingSpawn.position,
                    blessingSpawn.rotation);
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
    // �������������������������������������������������������������������������������������������������������������������������� StageManager.cs
    void SpawnWave(int waveIdx)
    {
        /* ������������������ 0) ��ȿ�� �˻� ������������������ */
        if (currentStageSettings == null)
        { Debug.LogError("[Stage] currentStageSettings �� NULL!"); return; }

        if (waveIdx < 0 || waveIdx >= currentStageSettings.waves.Length)
        { Debug.LogError($"[Stage] �߸��� waveIdx={waveIdx}"); return; }

        currentWave = waveIdx;
        Debug.Log($"[Stage] ===== SpawnWave({waveIdx}) =====");

        /* ������������������ 1) SpawnArea ������ �ε� & ���̺� 0 ������ ���� ������������������ */
        var spawnAreaPrefab = Resources.Load<GameObject>(spawnAreaPrefabName);
        if (spawnAreaPrefab == null)
        {
            Debug.LogError($"[Stage] Resources.Load('{spawnAreaPrefabName}') ���� �� ���/�̸� Ȯ��!");
            return;
        }

        if (waveIdx == 0)
        {
            var areaSettings = currentStageSettings.waves[0].spawnAreaSettings;
            Debug.Log($"[Stage] SpawnArea ���� ���� = {areaSettings.Length}");

            foreach (var s in areaSettings)
            {
                var areaGO = PhotonNetwork.Instantiate(spawnAreaPrefabName, s.position, spawnAreaRotation);
                Debug.Log($"[Stage]   SpawnArea �ν��Ͻ� {(areaGO ? "OK" : "FAIL")} @ {s.position}");

                if (!areaGO) continue;
                spawnAreaInstances.Add(areaGO);

                /* �ݰ� ����ȭ */
                var area = areaGO.GetComponent<SpawnArea>();
                if (area == null)
                    Debug.LogError("[Stage]   SpawnArea.cs ����!", areaGO);
                else
                {
                    area.SetRadius(s.radius);
                    Debug.Log($"[Stage]   �ݰ� r={s.radius}");
                }
            }
        }

        /* ������������������ 2) ���̵� �����ϸ� �� ��� ������������������ */
        int pCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        float diffMul = DifficultyManager.Instance.CountMul(pCnt);
        Debug.Log($"[Stage] playerCount={pCnt}, diffMul={diffMul:0.00}");

        /* ������������������ 3) ���� ���� ������������������ */
        for (int i = 0; i < spawnAreaInstances.Count; i++)
        {
            var spawner = spawnAreaInstances[i].GetComponentInChildren<MonsterSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning($"[Stage] MonsterSpawner ���� (area idx={i})", spawnAreaInstances[i]);
                continue;
            }

            /* ���� -> �����ϸ� ���� ���� + �α� */
            var baseInfos = currentStageSettings.waves[waveIdx].spawnAreaSettings[i].monsterSpawnInfos;
            var scaledInfos = baseInfos.Select(info =>
            {
                int cnt = Mathf.CeilToInt(info.count * diffMul);
                Debug.Log($"[Stage]   ��û : {info.monsterPrefabName} ��{cnt}");
                return new MonsterSpawnInfo { monsterPrefabName = info.monsterPrefabName, count = cnt };
            }).ToArray();

            spawner.SpawnMonsters(scaledInfos);
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
                if (!isEndStage)
                {
                    if (blessingNPC != null)
                    {
                        PhotonNetwork.Instantiate(
                        blessingNPCPrefabName,
                        blessingSpawn.position,
                        blessingSpawn.rotation);
                    }
                }
                for (int i = 0; i < isImmediateSpawnList.Count; i++)
                {
                    if (!isImmediateSpawnList[i])
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
