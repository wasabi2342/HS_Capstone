using UnityEngine;

[CreateAssetMenu(fileName = "SettingData", menuName = "Scriptable Objects/SettingData")]
public class SettingData : ScriptableObject
{
    public bool tutorialCompleted = false;
    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public FullScreenMode screenMode = FullScreenMode.FullScreenWindow;
    public Vector2Int resolution = new Vector2Int(1920, 1080);
}
