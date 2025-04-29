using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "NewSpawnSettings", menuName = "StageSettings Data")]
public class WaveSetting
{
    public SpawnAreaSetting[] spawnAreaSettings; 
}
public class StageSettings : ScriptableObject
{
    public WaveSetting[] waves; // ���������� ���̺� ����
}