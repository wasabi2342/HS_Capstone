using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class StageManager : MonoBehaviourPun
{
    [Header("Prefabs (Resources ������ �־�� ��)")]
    public string spawnAreaPrefabName = "SpawnArea"; // SpawnArea ������ �̸�

    [Header("Spawn Area Settings")]
    public Vector3[] spawnAreaPositions;
    public Quaternion spawnAreaRotation = Quaternion.identity; // �⺻ ȸ�� ��

    // ������ SpawnArea �ν��Ͻ��� ������ ����Ʈ
    private List<GameObject> spawnAreaInstances = new List<GameObject>();

    private void Start()
    {
        // Master Client������ �������� ����
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnStage();
        }
    }

    void SpawnStage()
    {
        // ������ �� ��ġ���� SpawnArea ������ ����
        foreach (Vector3 pos in spawnAreaPositions)
        {
            GameObject spawnAreaInstance = PhotonNetwork.Instantiate(spawnAreaPrefabName, pos, spawnAreaRotation);
            spawnAreaInstances.Add(spawnAreaInstance);
        }
    }
}
