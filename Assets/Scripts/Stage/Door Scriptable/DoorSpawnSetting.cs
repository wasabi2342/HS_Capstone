using UnityEngine;

[System.Serializable]
public class DoorSpawnSetting
{
    // 문이 생성될 위치
    public Vector3 position;
    // 문이 생성될 회전값 (기본값은 Quaternion.identity)
    public Quaternion rotation = Quaternion.identity;
    // 문에 할당될 보상 타입 
    public RewardType rewardType;    
}
