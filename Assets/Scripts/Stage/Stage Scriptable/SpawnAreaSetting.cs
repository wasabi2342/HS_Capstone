using UnityEngine;

[System.Serializable]
public class SpawnAreaSetting
{
    public Vector3 position;         // 스폰 영역 중심 위치
    public float radius;         // 해당 영역의 반경
    public int monsterSpawnCount;  // 영역 내 몬스터 스폰 수
}
