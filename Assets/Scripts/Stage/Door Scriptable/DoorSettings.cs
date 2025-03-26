using UnityEngine;

[CreateAssetMenu(fileName = "NewDoorSettings", menuName = "Door Settings")]
public class DoorSettings : ScriptableObject
{
    // Resources 폴더에 있어야 PhotonNetwork.Instantiate가 올바르게 작동합니다.
    public GameObject doorPrefab;

    // 문이 생성될 위치 및 회전값 설정 목록
    public DoorSpawnSetting[] doorSpawnSettings;

    // 모든 플레이어가 상호작용한 후 대기 시간 (초)
    public float waitTime = 5f;
}
