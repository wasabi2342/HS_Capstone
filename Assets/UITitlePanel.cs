using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UITitlePanel : UIBase
{
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Image fadeImage;
    [SerializeField]
    private Text pressAnyKey;
    [SerializeField]
    private Text title;

    private bool anyKeyInput = false;

    private void OnDisable()
    {
        InputManager.Instance.PlayerInput.actions["AnyKey"].started -= ctx => AnyKeyInput(ctx);
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
    }

    void Start()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
        InputManager.Instance.PlayerInput.actions["AnyKey"].started += ctx => AnyKeyInput(ctx);

        StartCoroutine(FadeTitle());
        StartCoroutine(PressAnyKeyBlinkingEffect());
    }

    public void AnyKeyInput(InputAction.CallbackContext ctx)
    {
        anyKeyInput = true;
    }

    IEnumerator FadeTitle()
    {
        yield return StartCoroutine(FadeImageAlpha(fadeImage, 1f, 0f, 0.5f));

        while (true)
        {
            yield return null;

            if (anyKeyInput)
            {
                break;
            }
        }

        pressAnyKey.gameObject.SetActive(false);
        title.gameObject.SetActive(false);

        // backgroundImage의 스케일을 0.6으로 축소
        backgroundImage.transform.DOScale(1f, 1f).SetEase(Ease.InOutSine);  // 1초 동안 크기 축소

        yield return new WaitForSeconds(0.5f);

        // fadeImage로 1초 동안 페이드 아웃
        yield return StartCoroutine(FadeImageAlpha(fadeImage, 0f, 1f, 0.5f));  // 1초 동안 페이드 아웃

        // 1초 대기 후 UI 패널 열기
        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.OpenPanel<UiStartPanel>();
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

    IEnumerator PressAnyKeyBlinkingEffect()
    {
        // 시간값 설정
        float fadeDuration = 1f;  // 흐림 및 선명해지는 시간
        float scaleDuration = 1f;  // 크기 변화 시간

        // 무한 반복
        while (!anyKeyInput)  // 키 입력이 있을 경우 애니메이션 종료
        {
            // 흐림과 선명해짐의 알파 변화
            pressAnyKey.DOFade(0.3f, fadeDuration).SetEase(Ease.InOutSine);  // 흐려짐
            //pressAnyKey.transform.DOScale(1.2f, scaleDuration).SetEase(Ease.InOutSine);  // 커짐
            yield return new WaitForSeconds(fadeDuration);

            pressAnyKey.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);  // 선명해짐
           // pressAnyKey.transform.DOScale(1f, scaleDuration).SetEase(Ease.InOutSine);  // 원래 크기로 돌아옴
            yield return new WaitForSeconds(fadeDuration);
        }
    }
}
