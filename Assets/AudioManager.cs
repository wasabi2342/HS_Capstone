using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Range(0f, 1f)]
    public float masterVolume = 1.0f;

    private Dictionary<string, EventInstance> loopingSounds = new Dictionary<string, EventInstance>();
    private string currentBGMPath = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1. OneShot 사운드 재생 (위치 적용)
    public void PlayOneShot(string eventPath, Vector3 position)
    {
        var attributes = RuntimeUtils.To3DAttributes(position);
        EventInstance instance = RuntimeManager.CreateInstance(eventPath);
        instance.set3DAttributes(attributes);
        instance.setVolume(masterVolume);
        instance.start();
        instance.release();
    }

    // 2. 루프 사운드 재생
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

    // 3. 루프 사운드 정지
    public void StopLoop(string eventPath)
    {
        if (loopingSounds.TryGetValue(eventPath, out EventInstance instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            loopingSounds.Remove(eventPath);
        }
    }

    // 4. 볼륨 변경
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        foreach (var pair in loopingSounds)
        {
            pair.Value.setVolume(masterVolume);
        }
    }

    // 경로 기반 헬퍼 메서드 (폴더 구조 반영)
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

    // BGM 사운드 재생 (OneShot용)
    public void PlayBGM(string bgmName, Vector3 position)
    {
        PlayOneShot($"event:/BGM/{bgmName}", position);
    }

    // BGM 루프 사운드 재생 (중복 재생 방지)
    public void PlayBGMLoop(string bgmName, Vector3 position)
    {
        string path = $"event:/BGM/{bgmName}";

        // 이미 동일 BGM이 재생 중이면 무시
        if (currentBGMPath == path && loopingSounds.ContainsKey(path))
            return;

        // 다른 BGM이 재생 중이면 정지
        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
        }

        currentBGMPath = path;
        PlayLoop(path, position);
    }

    // BGM 루프 사운드 정지
    public void StopBGMLoop(string bgmName)
    {
        string path = $"event:/BGM/{bgmName}";
        StopLoop(path);
        if (currentBGMPath == path)
            currentBGMPath = "";
    }

    // 현재 재생 중인 BGM 정지
    public void StopCurrentBGM()
    {
        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
            currentBGMPath = "";
        }
    }
}
