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
    private Image titleImage;

    private bool anyKeyInput = false;
    private System.Action<InputAction.CallbackContext> anyKeyCallback;

    private void OnDisable()
    {
        InputManager.Instance.PlayerInput.actions["AnyKey"].started -= anyKeyCallback;
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);
        //UIManager.Instance.SetCanvasSize(new Vector2(1920f, 1080f));
    }

    void Start()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);

        anyKeyCallback = AnyKeyInput;

        InputManager.Instance.PlayerInput.actions["AnyKey"].started += anyKeyCallback;
        //UIManager.Instance.SetCanvasSize(new Vector2(15040f, 4611.875f));

        StartCoroutine(FadeTitle());
        StartCoroutine(PressAnyKeyBlinkingEffect());
    }

    public void AnyKeyInput(InputAction.CallbackContext ctx)
    {
        anyKeyInput = true;
        AudioManager.Instance.PlayBGM("gamestart", transform.position);
    }

    IEnumerator FadeTitle()
    {
        yield return StartCoroutine(FadeImageAlpha(fadeImage, 1f, 0f, 0.5f));
        yield return StartCoroutine(FadeImageAlpha(titleImage, 0f, 1f, 0.5f));


        while (true)
        {
            yield return null;

            if (anyKeyInput)
            {
                break;
            }
        }

        pressAnyKey.gameObject.SetActive(false);
        titleImage.gameObject.SetActive(false);

        
        backgroundImage.transform.DOScale(1f, 1f).SetEase(Ease.InOutSine);
        // ��ġ�� ���� 200��ŭ �ø��� (���� ��ġ ����)
        RectTransform rectTransform = backgroundImage.GetComponent<RectTransform>();
        Vector2 newPos = rectTransform.anchoredPosition + new Vector2(0, 400);
        rectTransform.DOAnchorPos(newPos, 1f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.5f);

        // fadeImage�� 1�� ���� ���̵� �ƿ�
        yield return StartCoroutine(FadeImageAlpha(fadeImage, 0f, 1f, 0.5f));  // 1�� ���� ���̵� �ƿ�

        // 1�� ��� �� UI �г� ����
        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.OpenPanelInOverlayCanvas<UiStartPanel>();
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

        // ����
        color.a = to;
        image.color = color;
    }

    IEnumerator PressAnyKeyBlinkingEffect()
    {
        // �ð��� ����
        float fadeDuration = 1f;  // �帲 �� ���������� �ð�
        float scaleDuration = 1f;  // ũ�� ��ȭ �ð�

        // ���� �ݺ�
        while (!anyKeyInput)  // Ű �Է��� ���� ��� �ִϸ��̼� ����
        {
            // �帲�� ���������� ���� ��ȭ
            pressAnyKey.DOFade(0.3f, fadeDuration).SetEase(Ease.InOutSine);  // �����
            //pressAnyKey.transform.DOScale(1.2f, scaleDuration).SetEase(Ease.InOutSine);  // Ŀ��
            yield return new WaitForSeconds(fadeDuration);

            pressAnyKey.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);  // ��������
           // pressAnyKey.transform.DOScale(1f, scaleDuration).SetEase(Ease.InOutSine);  // ���� ũ��� ���ƿ�
            yield return new WaitForSeconds(fadeDuration);
        }
    }
}
