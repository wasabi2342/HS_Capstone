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

    /* �������������������� �ʱ�ȭ �������������������� */
    public void Init(Transform owner, Vector3 worldOffset)
    {
        target = owner;
        offset = worldOffset;
        cam = Camera.main;
    }

    /* �������������������� �ۺ� API �������������������� */
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

        if (Mathf.Approximately(norm, 0f) || atStep)     // 80��60��40��20��0 %
        {
            TriggerShake();
            if (isShield && Mathf.Approximately(norm, 0f))
                SpawnBreakFx();
        }
    }

    /* �������������������� ���� �ڷ�ƾ & ���� �������������������� */
    IEnumerator Slide(Image front, Image back, float targetFill)
    {
        front.fillAmount = targetFill;           // ��� ����
        yield return new WaitForSeconds(0.25f);  // ������ ������ ����
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

    /* �� ��鸲 + ���� �ı� FX �� */
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
