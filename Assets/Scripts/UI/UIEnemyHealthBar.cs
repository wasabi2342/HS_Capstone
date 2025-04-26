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
}
