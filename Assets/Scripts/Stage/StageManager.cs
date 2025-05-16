using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

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
    private List<GameObject> spawnAreaInstances = new List<GameObject>();
    private Quaternion spawnAreaRotation = Quaternion.Euler(0, 0, 0); // 기본 회전값 (0도)
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
            //PhotonNetwork.Instantiate(blessingNPCPrefabName, Vector3.zero, Quaternion.identity);
            //PhotonNetwork.Instantiate(doorPrefabName, new Vector3(10,0,0), Quaternion.identity);
            //if (coopOrBetrayNPCPos != null)
            //{
            //    PhotonNetwork.Instantiate(CoopOrBetrayNPCPrefabName, coopOrBetrayNPCPos.position, Quaternion.identity);
            //}

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
    void SpawnWave(int waveIdx)
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;            // 현재 참가자 수
        float diffScale = 1f + (playerCount - 1) * 0.2f;                    // 예: 1명→1.0, 2명→1.2, …, 4명→1.6

        
        var wave = currentStageSettings.waves[waveIdx];
        for (int i = 0; i < spawnAreaInstances.Count; i++)
        {
            var spawner = spawnAreaInstances[i].GetComponentInChildren<MonsterSpawner>();
            // 원본 infos
            var baseInfos = wave.spawnAreaSettings[i].monsterSpawnInfos;
            // 스케일링된 infos 생성
            var scaledInfos = baseInfos
                .Select(info => new MonsterSpawnInfo
                {
                    monsterPrefabName = info.monsterPrefabName,
                    count = Mathf.CeilToInt(info.count * diffScale)
                })
                .ToArray();
            spawner.SpawnMonsters(scaledInfos);
        }
    }

    public void OnAllMonsterCleared()
    {
        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트만 실행
        if (currentWave < currentStageSettings.waves.Length - 1)
        {
            SpawnWave(currentWave + 1); // 다음 웨이브 스폰
        }
        else
        {
            AreAllMonstersCleared(); // 모든 몬스터 제거 확인
        }
    }

    public bool AreAllMonstersCleared()
    {
        Debug.Log("ActiveMonsterCount: " + EnemyFSM.ActiveMonsterCount);
        bool cleared = EnemyFSM.ActiveMonsterCount == 0;
        if (cleared)
        {
            Debug.Log("모든 몬스터가 제거되었습니다.");
            if (PhotonNetwork.IsMasterClient)
            {
                if (isEndStage && PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    PhotonNetworkManager.Instance.EndGameInSoloPlay();
                }
                // Resources 폴더에서 doorPrefab을 로드
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
