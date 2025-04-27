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

    // 1. OneShot ���� ��� (��ġ ����)
    public void PlayOneShot(string eventPath, Vector3 position)
    {
        var attributes = RuntimeUtils.To3DAttributes(position);
        EventInstance instance = RuntimeManager.CreateInstance(eventPath);
        instance.set3DAttributes(attributes);
        instance.setVolume(masterVolume);
        instance.start();
        instance.release();
    }

    // 2. ���� ���� ���
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

    // 3. ���� ���� ����
    public void StopLoop(string eventPath)
    {
        if (loopingSounds.TryGetValue(eventPath, out EventInstance instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            loopingSounds.Remove(eventPath);
        }
    }

    // 4. ���� ����
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        foreach (var pair in loopingSounds)
        {
            pair.Value.setVolume(masterVolume);
        }
    }

    // ��� ��� ���� �޼��� (���� ���� �ݿ�)
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

    // BGM ���� ��� (OneShot��)
    public void PlayBGM(string bgmName, Vector3 position)
    {
        PlayOneShot($"event:/BGM/{bgmName}", position);
    }

    // BGM ���� ���� ��� (�ߺ� ��� ����)
    public void PlayBGMLoop(string bgmName, Vector3 position)
    {
        string path = $"event:/BGM/{bgmName}";

        // �̹� ���� BGM�� ��� ���̸� ����
        if (currentBGMPath == path && loopingSounds.ContainsKey(path))
            return;

        // �ٸ� BGM�� ��� ���̸� ����
        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
        }

        currentBGMPath = path;
        PlayLoop(path, position);
    }

    // BGM ���� ���� ����
    public void StopBGMLoop(string bgmName)
    {
        string path = $"event:/BGM/{bgmName}";
        StopLoop(path);
        if (currentBGMPath == path)
            currentBGMPath = "";
    }

    // ���� ��� ���� BGM ����
    public void StopCurrentBGM()
    {
        if (!string.IsNullOrEmpty(currentBGMPath))
        {
            StopLoop(currentBGMPath);
            currentBGMPath = "";
        }
    }
}
