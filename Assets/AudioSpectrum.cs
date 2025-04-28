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
    public float volume = 1.0f;

    [Header("Spectrum Settings")]
    public int numBands = 64;
    public Image barPrefab;
    public Image glowPrefab;
    [Tooltip("원 중심부터 시작하는 기본 반지름")]
    public float radius = 100f;
    [Tooltip("스펙트럼 높이 배율")]
    public float heightMultiplier = 100f;

    [Header("UI Hierarchy")]
    public RectTransform contentParent;

    [Header("Color Settings")]
    public Color startColor = Color.cyan;
    public Color middleColor = Color.white;
    public Color endColor = Color.magenta;
    [Range(0f, 100f)]
    public float colorIntensityMultiplier = 5f; // 기본값 증가
    [Range(1f, 50f)]
    public float colorTransitionSpeed = 10f; // 기본값 증가
    [Range(0.1f, 10f)]
    public float colorSensitivity = 2.0f; // 색상 민감도 추가
    [Range(0f, 1f)]
    public float minColorIntensity = 0.1f; // 최소 색상 강도 추가

    [Header("Frequency Range")]
    public float minFrequency = 20f;
    public float maxFrequency = 20000f;

    [Header("Amplitude Scaling")]
    [Range(0.01f, 200f)]
    public float lowFrequencyScale = 1f;
    [Range(0.01f, 200f)]
    public float highFrequencyScale = 1f;

    private Image[] bars;
    private Image[] glows;
    private Vector3[] initialScales;
    private Color[] currentBarColors; // 현재 막대 색상 저장
    private Color[] currentGlowColors; // 현재 글로우 색상 저장
    private bool useGlow = false; // 글로우 사용 여부

    private EventInstance eventInstance;
    private DSP fft;
    private int fftSize = 1024;
    private int sampleRate;

    void Start()
    {
        if (contentParent == null || barPrefab == null)
        {
            UnityEngine.Debug.LogWarning("ContentParent 또는 BarPrefab이 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // 글로우 사용 여부 확인
        useGlow = (glowPrefab != null);

        bars = new Image[numBands];
        initialScales = new Vector3[numBands];
        currentBarColors = new Color[numBands]; // 색상 배열 초기화
        
        // 글로우 프리펩이 있는 경우에만 초기화
        if (useGlow)
        {
            glows = new Image[numBands];
            currentGlowColors = new Color[numBands]; // 글로우 색상 배열 초기화
        }

        for (int i = 0; i < numBands; i++)
        {
            float angle = (360f / numBands) * i;

            bars[i] = Instantiate(barPrefab, contentParent);
            bars[i].rectTransform.localPosition = Vector3.zero;
            bars[i].rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            initialScales[i] = bars[i].rectTransform.localScale;

            // 초기 색상 설정
            currentBarColors[i] = startColor;

            // 글로우 프리펩이 있는 경우에만 생성
            if (useGlow)
            {
                glows[i] = Instantiate(glowPrefab, contentParent);
                glows[i].rectTransform.localPosition = Vector3.zero;
                glows[i].rectTransform.localEulerAngles = new Vector3(0, 0, angle);
                glows[i].rectTransform.localScale = initialScales[i] * 1.1f;
                Color glowColor = Color.Lerp(startColor, endColor, i / (float)(numBands - 1));
                glowColor.a = 0.5f;
                glows[i].color = glowColor;
                currentGlowColors[i] = glowColor;
            }
        }

        eventInstance = RuntimeManager.CreateInstance(eventPath);
        eventInstance.setVolume(volume);
        eventInstance.start();

        ChannelGroup master;
        RuntimeManager.CoreSystem.getMasterChannelGroup(out master);
        RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out fft);
        fft.setParameterInt((int)DSP_FFT.WINDOWSIZE, fftSize);
        master.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, fft);

        RuntimeManager.CoreSystem.getSoftwareFormat(out sampleRate, out _, out _);
    }

    void Update()
    {
        if (!fft.hasHandle()) return;

        System.IntPtr data;
        uint length;
        fft.getParameterData((int)DSP_FFT.SPECTRUMDATA, out data, out length);
        DSP_PARAMETER_FFT fftData = (DSP_PARAMETER_FFT)System.Runtime.InteropServices.Marshal.PtrToStructure(data, typeof(DSP_PARAMETER_FFT));
        if (fftData.numchannels == 0) return;

        float[] rawSpectrum = fftData.spectrum[0];
        int spectrumLength = fftData.length;

        for (int i = 0; i < numBands; i++)
        {
            // ⭐ 리니어 주파수 매핑 ⭐
            float freqT = i / (float)(numBands - 1); // 't' 변수명을 'freqT'로 변경
            float targetFrequency = Mathf.Lerp(minFrequency, maxFrequency, freqT);

            float frequencyPerBin = sampleRate / (float)fftSize;
            int index = Mathf.RoundToInt(targetFrequency / frequencyPerBin);
            index = Mathf.Clamp(index, 0, spectrumLength - 1);

            float value = rawSpectrum[index];

            float normalizedIndex = i / (float)(numBands - 1);
            float scaleFactor = Mathf.Lerp(lowFrequencyScale, highFrequencyScale, normalizedIndex);
            float targetScaleY = initialScales[i].y * (1 + value * scaleFactor * heightMultiplier * 0.1f);

            Vector3 currentScale = bars[i].rectTransform.localScale;
            currentScale.y = Mathf.Lerp(currentScale.y, targetScaleY, Time.deltaTime * 30f);
            bars[i].rectTransform.localScale = currentScale;

            // 중심에서 radius + bar길이 만큼 위치
            float halfHeight = bars[i].rectTransform.rect.height * currentScale.y * 0.5f;
            float offset = radius + halfHeight;
            bars[i].rectTransform.localPosition = bars[i].rectTransform.up * offset;

            // 색상 민감도와 최소값 적용
            float amplitudeFactor = Mathf.Clamp01(
                minColorIntensity + (value * colorIntensityMultiplier * colorSensitivity)
            );
            
            // 색상 전환을 더 역동적으로 만들기
            Color targetBarColor;
            if (amplitudeFactor <= 0.5f)
            {
                // 시작 색상에서 중간 색상으로 더 강하게 보간
                float tColor1 = Mathf.Pow(amplitudeFactor * 2f, 0.8f); // 't'에서 'tColor1'로 변경
                targetBarColor = Color.Lerp(startColor, middleColor, tColor1);
            }
            else
            {
                // 중간 색상에서 종료 색상으로 더 강하게 보간
                float tColor2 = Mathf.Pow((amplitudeFactor - 0.5f) * 2f, 0.8f); // 't'에서 'tColor2'로 변경
                targetBarColor = Color.Lerp(middleColor, endColor, tColor2);
            }
            
            // 더 빠른 색상 전환을 위해 가중치 적용
            float transitionWeight = Time.deltaTime * colorTransitionSpeed;
            // 진폭이 클수록 색상 전환도 빨리 진행되도록
            transitionWeight *= (1f + amplitudeFactor);
            
            // 현재 색상에서 목표 색상으로 부드럽게 전환
            currentBarColors[i] = Color.Lerp(currentBarColors[i], targetBarColor, transitionWeight);
            bars[i].color = currentBarColors[i];

            // 글로우 프리펩이 있는 경우에만 처리
            if (useGlow && glows != null && glows[i] != null)
            {
                glows[i].rectTransform.localScale = currentScale * 1.1f;
                glows[i].rectTransform.localPosition = bars[i].rectTransform.localPosition;
                
                // 글로우 목표 색상 계산 (바와 동일한 로직 적용)
                Color targetGlowColor;
                if (amplitudeFactor <= 0.5f)
                {
                    float tGlow1 = Mathf.Pow(amplitudeFactor * 2f, 0.8f); // 't'에서 'tGlow1'로 변경
                    targetGlowColor = Color.Lerp(startColor, middleColor, tGlow1);
                }
                else
                {
                    float tGlow2 = Mathf.Pow((amplitudeFactor - 0.5f) * 2f, 0.8f); // 't'에서 'tGlow2'로 변경
                    targetGlowColor = Color.Lerp(middleColor, endColor, tGlow2);
                }
                targetGlowColor.a = 0.5f;
                
                // 바와 동일한 전환 가중치 적용
                currentGlowColors[i] = Color.Lerp(currentGlowColors[i], targetGlowColor, transitionWeight);
                glows[i].color = currentGlowColors[i];
            }
        }
    }

    void OnDestroy()
    {
        if (eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        if (fft.hasHandle())
        {
            fft.release();
        }
    }
}