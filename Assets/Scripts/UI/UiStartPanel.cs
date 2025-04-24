using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiStartPanel : UIBase
{
    [SerializeField]
    private Button singlePlayButton;
    [SerializeField]
    private Button multiPlayButton;
    [SerializeField]
    private Button settingButton;
    [SerializeField]
    private Button quitButton;
    [SerializeField]
    private Image fadeImage;

    void Start()
    {
        StartCoroutine(FadeImageAlpha(fadeImage, 1f, 0f, 0.5f));

        Init();
    }

    private void OnClikedMultiPlayButton()
    {
        PhotonNetworkManager.Instance.ConnectPhoton();
        UIManager.Instance.OpenPanel<UILobbyPanel>();
    }

    public override void Init()
    {
        multiPlayButton.onClick.AddListener(OnClikedMultiPlayButton);
        quitButton.onClick.AddListener(QuitGame);
        settingButton.onClick.AddListener(() => UIManager.Instance.OpenPopupPanel<UISettingPanel>());

        singlePlayButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseAllUI();
            UIManager.Instance.OpenPanel<UIRoomPanel>();
        }
        );
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

        // º¸Á¤
        color.a = to;
        image.color = color;
    }
}
