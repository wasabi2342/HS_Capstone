using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnSettings", menuName = "StageSettings Data")]
public class StageSettings : ScriptableObject
{
    public SpawnAreaSetting[] spawnAreaSettings;
}