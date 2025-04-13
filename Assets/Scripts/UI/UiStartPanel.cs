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

    void Start()
    {
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
}
