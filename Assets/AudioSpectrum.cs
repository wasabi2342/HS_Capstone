using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD;
using FMOD.Studio;

public class CircularAudioSpectrumYPlaneOutwardUIImageScaleCustomRange : MonoBehaviour
{
    [Header("FMOD Settings")]
    [EventRef]
    public string eventPath;

    [Header("Sound Settings")]
    [Range(0f, 1f)]
    [Tooltip("재생할 소리의 볼륨 (0 = 무음, 1 = 최대 볼륨)")]
    public float volume = 1.0f;

    [Header("Spectrum Settings")]
    public int numBands = 64;
    public Image barPrefab; // 원본 UI Image 프리팹
    public Image glowPrefab; // Glow 효과를 위한 UI Image 프리팹 (선택 사항)
    [Tooltip("원형 배치 시 반지름")]
    public float radius = 100f; // UI 좌표계에 맞게 조절 필요
    [Tooltip("자동으로 각도 간격 계산")]
    public bool autoCalculateAngle = true;
    public float heightMultiplier = 100f; // UI 스케일에 맞게 조절 필요

    [Header("UI Hierarchy")]
    public RectTransform contentParent;

    [Header("Color Settings")]
    public Color startColor = Color.cyan;
    public Color endColor = Color.magenta;
    [Range(0f, 100f)]
    public float colorIntensityMultiplier = 1f; // 색상 강도 조절

    [Header("Frequency Range")]
    public float minFrequency = 20f;
    public float maxFrequency = 20000f;

    [Header("Amplitude Scaling")]
    [Range(0.01f, 200f)]
    public float lowFrequencyScale = 1f;
    [Range(0.01f, 200f)]
    public float highFrequencyScale = 1f;

    private Image[] bars;
    private Image[] glows; // Glow 이미지 배열 (선택 사항)
    private EventInstance eventInstance;
    private DSP fft;
    private float[] spectrumData;
    private Vector3[] initialScales;
    private int fftSize = 1024;
    private int sampleRate;

    void Start()
    {
        bars = new Image[numBands];
        initialScales = new Vector3[numBands];

        if (contentParent == null)
        {
            enabled = false;
            return;
        }

        // Glow 프리팹이 할당되지 않았으면 glows 배열은 null로 유지
        if (glowPrefab != null)
        {
            glows = new Image[numBands];
        }

        for (int i = 0; i < numBands; i++)
        {
            // 원본 이미지 생성 및 배치
            bars[i] = Instantiate(barPrefab, contentParent);
            float angle = i * (360f / numBands);
            float radian = angle * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(radian);
            float y = radius * Mathf.Sin(radian);
            bars[i].rectTransform.localPosition = new Vector3(x, y, 0);
            bars[i].rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            initialScales[i] = bars[i].rectTransform.localScale;

            // Glow 이미지 생성 및 배치 (선택 사항)
            if (glowPrefab != null)
            {
                glows[i] = Instantiate(glowPrefab, contentParent);
                glows[i].rectTransform.localPosition = new Vector3(x, y, 0);
                glows[i].rectTransform.localEulerAngles = new Vector3(0, 0, angle);
                glows[i].rectTransform.localScale = initialScales[i] * 1.1f; // 약간 더 크게
                Color glowColor = Color.Lerp(startColor, endColor, i / (float)(numBands - 1));
                glowColor.a = 0.5f; // 투명도 조절
                glows[i].color = glowColor;
            }
        }

        // 이벤트 인스턴스 생성 후 인스펙터에서 설정한 볼륨 적용
        eventInstance = RuntimeManager.CreateInstance(eventPath);
        eventInstance.setVolume(volume);
        eventInstance.start();

        ChannelGroup master;
        RuntimeManager.CoreSystem.getMasterChannelGroup(out master);
        RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out fft);
        fft.setParameterInt((int)DSP_FFT.WINDOWSIZE, fftSize);
        master.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, fft);

        RuntimeManager.CoreSystem.getSoftwareFormat(out sampleRate, out _, out _);

        // spectrumData 배열의 크기는 fftSize로 변경
        spectrumData = new float[fftSize];
    }

    void Update()
    {
        if (!fft.hasHandle()) return;

        System.IntPtr data;
        uint length;
        fft.getParameterData((int)DSP_FFT.SPECTRUMDATA, out data, out length);

        DSP_PARAMETER_FFT fftData = (DSP_PARAMETER_FFT)System.Runtime.InteropServices.Marshal.PtrToStructure(
          data, typeof(DSP_PARAMETER_FFT));

        if (fftData.numchannels == 0) return;

        int spectrumLength = fftData.length;
        float[] rawSpectrum = fftData.spectrum[0];

        for (int i = 0; i < numBands; i++)
        {
            float normalizedFrequency = Mathf.Lerp(0f, 1f, i / (float)(numBands - 1));
            float logMinF = Mathf.Log(minFrequency) / Mathf.Log(2);
            float logMaxF = Mathf.Log(maxFrequency) / Mathf.Log(2);
            float targetFrequency = Mathf.Pow(2, Mathf.Lerp(logMinF, logMaxF, normalizedFrequency));

            float frequencyPerBin = sampleRate / (float)fftSize;
            int index = Mathf.RoundToInt(targetFrequency / frequencyPerBin);
            index = Mathf.Clamp(index, 0, spectrumLength - 1);

            float value = rawSpectrum[index];

            float normalizedIndex = i / (float)(numBands - 1);
            float scaleFactor = Mathf.Lerp(lowFrequencyScale, highFrequencyScale, normalizedIndex);
            float targetScaleX = initialScales[i].x * (1 + value * scaleFactor * heightMultiplier * 0.1f);

            Vector3 currentScale = bars[i].rectTransform.localScale;
            currentScale.x = Mathf.Lerp(currentScale.x, targetScaleX, Time.deltaTime * 30f);
            bars[i].rectTransform.localScale = currentScale;

            float amplitudeFactor = Mathf.Clamp01(value * colorIntensityMultiplier);
            bars[i].color = Color.Lerp(startColor, endColor, amplitudeFactor);

            if (glows != null && glows[i] != null)
            {
                glows[i].rectTransform.localScale = currentScale * 1.1f;
                Color glowColor = Color.Lerp(startColor, endColor, amplitudeFactor);
                glowColor.a = 0.5f;
                glows[i].color = glowColor;
            }
        }
    }

    void OnDestroy()
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();

        if (fft.hasHandle())
        {
            fft.release();
        }
    }
}
