using Photon.Pun;
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
    private Button tutoPlayButton;
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
        if (DataManager.Instance.settingData.tutorialCompleted == false)
        {
            OnClikedSinglePlayButton();
        }
        else
        {
            PhotonNetworkManager.Instance.ConnectPhoton();
            UIManager.Instance.OpenPanelInOverlayCanvas<UILobbyPanel>();
        }
    }

    private void OnClikedSinglePlayButton()
    {
        PhotonNetworkManager.Instance.ConnectPhotonToSinglePlay();

        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("싱글 플레이 모드 - 씬 로드");
            if (DataManager.Instance.settingData.tutorialCompleted == false)
                PhotonNetwork.LoadLevel("Tutorial"); // 튜토리얼로
            else
                UIManager.Instance.OpenPanelInOverlayCanvas<UIRoomPanel>(); //나중에 넣기

        }
        //UIManager.Instance.CloseAllUI();
    }

    public override void Init()
    {
        PhotonNetwork.Disconnect();

        multiPlayButton.onClick.AddListener(OnClikedMultiPlayButton);
        quitButton.onClick.AddListener(QuitGame);
        settingButton.onClick.AddListener(() => UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIMenuPanel>());

        singlePlayButton.onClick.AddListener(OnClikedSinglePlayButton);

        string nickname = PlayerPrefs.GetString("Nickname");

        if (nickname != "")
        {
            PhotonNetworkManager.Instance.SetNickname(nickname);
        }
        else
        {
            UIManager.Instance.OpenPopupPanelInOverlayCanvas<UISetNicknamePanel>();
        }
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

        // 보정
        color.a = to;
        image.color = color;
    }
}
