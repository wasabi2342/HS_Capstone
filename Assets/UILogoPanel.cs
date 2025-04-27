using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UILogoPanel : UIBase
{
    [SerializeField]
    private Image logoImage;
    [SerializeField]
    private Image fadeImage;

    void Start()
    {
        StartCoroutine(FadeLogo());
    }

    IEnumerator FadeLogo()
    {
        yield return new WaitForSeconds(0.5f);

        // 알파값 0 → 1 (0.5초)
        yield return StartCoroutine(FadeImageAlpha(logoImage, 0f, 1f, 0.5f));

        // 1초 유지
        yield return new WaitForSeconds(2f);

        // 알파값 1 → 0 (0.5초)
        yield return StartCoroutine(FadeImageAlpha(logoImage, 1f, 0f, 0.5f));

        // 브금 재생 시작
        AudioManager.Instance.PlayBGMLoop("MainTitleOST", Vector3.zero);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(FadeImageAlpha(fadeImage, 0f, 1f, 0.5f));

        UIManager.Instance.OpenPanel<UITitlePanel>();
    }

    IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
    {
        float timer = 0f;
        Color color = image.color;
        color.a = from;
        image.color = color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, timer / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }

        // 보정
        color.a = to;
        image.color = color;
    }

}
