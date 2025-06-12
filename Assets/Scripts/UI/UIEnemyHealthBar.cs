using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIEnemyHealthBar : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] Image hpFill;
    [SerializeField] Image hpDelay;

    [Header("Shield")]
    [SerializeField] GameObject shieldGrp;
    [SerializeField] Image shdFill;
    [SerializeField] Image shdDelay;

    [Header("Damage Text Prefab")]
    [SerializeField] GameObject damageTextPrefab;
    /// <summary>
    /// MasterClient 가 HP 계산 후, 모든 클라이언트에 HP 비율 전송
    /// </summary>
    Transform target;
    Vector3 offset;
    Camera cam;

    /* ────────── 초기화 ────────── */
    public void Init(Transform owner, Vector3 worldOffset)
    {
        target = owner;
        offset = worldOffset;
        cam = Camera.main;
    }

    /* ────────── 퍼블릭 API ────────── */
    public void SetHP(float norm) => StartCoroutine(Slide(hpFill, hpDelay, norm));
    public void SetShield(float norm)
    {
        shieldGrp.SetActive(norm > 0f);
        if (norm > 0f) StartCoroutine(Slide(shdFill, shdDelay, norm));
    }

    public void CheckThreshold(float norm, bool isShield)
    {
        const float STEP = 0.2f;                         // 20 %
        bool atStep = Mathf.Abs(norm / STEP - Mathf.Round(norm / STEP)) < 0.001f;

        if (Mathf.Approximately(norm, 0f) || atStep)     // 80·60·40·20·0 %
        {
            TriggerShake();
            if (isShield && Mathf.Approximately(norm, 0f))
                SpawnBreakFx();
        }
    }

    /* ────────── 내부 코루틴 & 연출 ────────── */
    IEnumerator Slide(Image front, Image back, float targetFill)
    {
        front.fillAmount = targetFill;           // 즉시 감소
        yield return new WaitForSeconds(0.25f);  // 딜레이 게이지 지연
        float start = back.fillAmount;
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            back.fillAmount = Mathf.Lerp(start, targetFill, t / 0.4f);
            yield return null;
        }
        back.fillAmount = targetFill;
    }

    void LateUpdate()
    {
        if (!target) return;
        transform.position = target.position + offset;
        transform.forward = cam.transform.forward;
        transform.localScale = Vector3.one;
    }

    /* ─ 흔들림 + 쉴드 파괴 FX ─ */
    void TriggerShake() => StartCoroutine(CoShake());

    IEnumerator CoShake()
    {
        Vector3 origin = transform.localPosition;
        float t = 0, dur = 0.3f;
        while (t < dur)
        {
            t += Time.deltaTime;
            transform.localPosition = origin + (Vector3)Random.insideUnitCircle * 0.05f;
            yield return null;
        }
        transform.localPosition = origin;
    }

    void SpawnBreakFx()
    {
        var fx = Resources.Load<GameObject>("ShieldBreakFx");
        if (fx) Instantiate(fx, target.position + Vector3.up * 1.2f, Quaternion.identity);
    }

    /// <summary>
    /// 몬스터가 받은 데미지를 숫자로 띄워 줍니다.
    /// </summary>
    public void ShowDamage(float damage)
    {
        if (damageTextPrefab == null) return;

        // 1) 텍스트 인스턴스 생성
        var go = Instantiate(damageTextPrefab, Vector3.zero, Quaternion.identity, this.transform);
        // 2) 위치: HP바 바로 위
        go.transform.position = transform.position + Vector3.up * 0.2f;
        Vector3 ps = this.transform.lossyScale;
        go.transform.localScale = Vector3.one * 0.01f;
        go.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
        // 3) 텍스트 세팅
        Text txt = go.GetComponentInChildren<Text>();
        txt.text = Mathf.RoundToInt(damage).ToString();
        // 4) 위로 떠오르며 페이드아웃
        StartCoroutine(CoFloatAndFade(go, txt));
    }

    IEnumerator CoFloatAndFade(GameObject go, Text txt)
    {
        float t = 0f, dur = 0.8f;
        Vector3 start = go.transform.position;
        Color c0 = txt.color;

        while (t < dur)
        {
            t += Time.deltaTime;
            go.transform.position = start + Vector3.up * (t / dur * 0.5f);
            float a = Mathf.Lerp(1f, 0f, t / dur);
            txt.color = new Color(c0.r, c0.g, c0.b, a);
            yield return null;
        }

        Destroy(go);
    }
}
