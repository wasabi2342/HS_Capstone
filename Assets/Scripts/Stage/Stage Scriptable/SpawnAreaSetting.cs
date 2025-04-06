using UnityEngine;

[System.Serializable]
public class MonsterSpawnInfo
{
    public string monsterPrefabName; // Resources ���� �� ������ �̸�
    public int count;               // �� ���͸� �� ���� ��������
}

[System.Serializable]
public class SpawnAreaSetting
{
    public Vector3 position;        // ���� ���� �߽�
    public float radius;            // ���� �ݰ�

    // �� ���͸��� "������ �̸� + ���� ��"�� ������� �迭
    public MonsterSpawnInfo[] monsterSpawnInfos;
}
