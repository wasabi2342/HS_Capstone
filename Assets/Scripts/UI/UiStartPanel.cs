using UnityEngine;
using UnityEngine.UI;

public class UiStartPanel : MonoBehaviour
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
        multiPlayButton.onClick.AddListener(OnClikedMultiPlayButton);
    }

    private void OnClikedMultiPlayButton()
    {
        PhotonNetworkManager.Instance.ConnectPhoton();
        UIManager.Instance.OpenPanel(UIState.Lobby);
    }
}
