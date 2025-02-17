using UnityEngine;
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
    }
}
