using UnityEngine;

[System.Serializable]
public class MonsterSpawnInfo
{
    public string monsterPrefabName; // Resources 폴더 내 프리팹 이름
    public int count;               // 이 몬스터를 몇 마리 스폰할지
}

[System.Serializable]
public class SpawnAreaSetting
{
    public Vector3 position;        // 스폰 영역 중심
    public float radius;            // 스폰 반경

    // 각 몬스터마다 "프리팹 이름 + 스폰 수"를 묶어놓은 배열
    public MonsterSpawnInfo[] monsterSpawnInfos;
}
