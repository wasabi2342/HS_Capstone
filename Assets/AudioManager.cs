using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public enum SoundType
{
    defaultType,
    bgmType,
    SFXType
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    //fmod 사운드 버스
    private VCA vcaMaster;
    private VCA vcaBGM;
    private VCA vcaSFX;


    [Range(0f, 1f)]
    public float masterVolume = 1.0f;

    private Dictionary<string, EventInstance> loopingSounds = new Dictionary<string, EventInstance>();
    private string currentBGMPath = "";
    private EventInstance currentBGMInstance; // 현재 재생 중인 BGM EventInstance 저장

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }

        //fmod 사운드 버스
        vcaMaster = RuntimeManager.GetVCA("vca:/VCA_Master");
        vcaBGM = RuntimeManager.GetVCA("vca:/VCA_BGM");
        vcaSFX = RuntimeManager.GetVCA("vca:/VCA_SFX");

        if (DataManager.Instance != null && DataManager.Instance.settingData != null)
        {
            float master = DataManager.Instance.settingData.masterVolume;
            float bgm = DataManager.Instance.settingData.bgmVolume;
            float sfx = DataManager.Instance.settingData.sfxVolume;

            vcaMaster.setVolume(master);
            vcaBGM.setVolume(bgm);
            vcaSFX.setVolume(sfx);
        }

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Level"))
        {
            StopCurrentBGM();
        }
    }

    public void PlayOneShot(string eventPath, Vector3 position)
    {
        var attributes = RuntimeUtils.To3DAttributes(position);
        EventInstance instance = RuntimeManager.CreateInstance(eventPath);
        instance.set3DAttributes(attributes);
        instance.setVolume(masterVolume);
        instance.start();
        instance.release();
    }

    public void PlayLoop(string eventPath, Vector3 position)
    {
        if (loopingSounds.ContainsKey(eventPath))
        {
            Debug.LogWarning($"Looping sound already playing: {eventPath}");
            return;
        }

        var instance = RuntimeManager.CreateInstance(eventPath);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.setVolume(masterVolume);
        instance.start();
        loopingSounds[eventPath] = instance;
    }

    public void StopLoop(string eventPath)
    {
        if (loopingSounds.TryGetValue(eventPath, out EventInstance instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            loopingSounds.Remove(eventPath);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (vcaMaster.isValid())
            vcaMaster.setVolume(masterVolume);
    }


    public void PlayCharacterSFX(string characterType, string actionName, Vector3 position)
    {
        PlayOneShot($"event:/Character/{characterType}/{actionName}", position);
    }

    public void PlayMonsterSFX(string actionName, Vector3 position)
    {
        PlayOneShot($"event:/Character/Monster/{actionName}", position);
    }

    public void PlayCommonSFX(string soundName, Vector3 position)
    {
        PlayOneShot($"event:/Character/Common/{soundName}", position);
    }

    public void PlayUISFX(string sfxName, Vector3 position)
    {
        PlayOneShot($"event:/UI/{sfxName}", position);
    }

    public void PlayBGM(string bgmName, Vector3 position)
    {
        PlayOneShot($"event:/BGM/{bgmName}", position);
    }

    public void PlayBGMLoop(string bgmName, Vector3 position)
    {
        string path = $"event:/BGM/{bgmName}";

        if (currentBGMPath == path && loopingSounds.ContainsKey(path))
            return;

        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
            if (currentBGMInstance.isValid())
                currentBGMInstance.release();
        }

        currentBGMPath = path;
        currentBGMInstance = RuntimeManager.CreateInstance(path); // BGM EventInstance 저장
        currentBGMInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        currentBGMInstance.setVolume(masterVolume);
        currentBGMInstance.start();
        loopingSounds[path] = currentBGMInstance;
    }

    public void StopBGMLoop(string bgmName)
    {
        string path = $"event:/BGM/{bgmName}";
        StopLoop(path);
        if (currentBGMPath == path)
        {
            currentBGMPath = "";
            if (currentBGMInstance.isValid())
                currentBGMInstance.release();
        }
    }

    public void StopCurrentBGM()
    {
        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
            if (currentBGMInstance.isValid())
                currentBGMInstance.release();
            currentBGMPath = "";
        }
    }

    // 현재 재생 중인 BGM EventInstance를 반환하는 메서드
    public EventInstance GetCurrentBGMInstance()
    {
        return currentBGMInstance;
    }

    //사운드 버스 제어
    public void SetVCAMasterVolume(float volume)
    {
        vcaMaster.setVolume(volume);
    }

    public void SetVCABGMVolume(float volume)
    {
        vcaBGM.setVolume(volume);
    }

    public void SetVCASFXVolume(float volume)
    {
        vcaSFX.setVolume(volume);
    }

}