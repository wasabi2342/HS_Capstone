using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    [Header("Prefabs (Resources 폴더에 있어야 함)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea 프리팹 이름

    [Header("Spawn Area Settings")]
    public Vector3[] spawnAreaPositions;
    public Quaternion spawnAreaRotation = Quaternion.identity; // 기본 회전 값

    // 생성된 SpawnArea 인스턴스를 저장할 리스트
    private List<GameObject> spawnAreaInstances = new List<GameObject>();

    private void Start()
    {
        // Master Client에서만 스테이지 생성
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnStage();
        }
    }

    void SpawnStage()
    {
        // 지정된 각 위치마다 SpawnArea 프리팹 생성
        foreach (Vector3 pos in spawnAreaPositions)
        {
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate(spawnAreaPrefabName, pos, spawnAreaRotation);
            spawnAreaInstances.Add(spawnAreaInstance);
        }
    }
}
