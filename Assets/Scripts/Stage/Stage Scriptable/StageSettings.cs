using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "NewSpawnSettings", menuName = "StageSettings Data")]
public class WaveSetting
{
    public SpawnAreaSetting[] spawnAreaSettings; 
}
public class StageSettings : ScriptableObject
{
    public WaveSetting[] waves; // 스테이지의 웨이브 설정
}