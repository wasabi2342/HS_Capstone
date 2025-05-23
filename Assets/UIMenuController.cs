using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuPanel : UIBase
{
    [Header("���� ��ũ�� �� �׷�")]
    public GameObject optionList;
    public GameObject guideList;

    [Header("���� ���� �гε�")]
    public GameObject[] rightPanels; // Credit, GraphicOption, Character ��

    [SerializeField]
    private Button preButton;
    [SerializeField]
    private Button quitButton;
    [SerializeField]
    private Button gotoStartUIButton;
    [SerializeField]
    private Button escapeButton;
    [SerializeField]
    private TMP_Dropdown resolutionDropdown;
    [SerializeField]
    private TMP_Dropdown windowDropdown;

    [SerializeField]
    private Slider masterVolumeSlider;
    [SerializeField]
    private Slider bgmVolumeSlider;
    [SerializeField]
    private Slider sfxVolumeSlider;

    private Action<InputAction.CallbackContext> closeUIAction;

    private void Start()
    {
        // ������ �� ��� RightPanel ���� ����
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }

        Init();
    }

    public override void Init()
    {
        if (RoomManager.Instance == null)
        {
            escapeButton.gameObject.SetActive(false);
        }
        else
        {
            escapeButton.onClick.AddListener(() =>
            {
                RoomManager.Instance.EscapePlayer();
                if (UIManager.Instance.ReturnPeekUI() as UIMenuPanel)
                {
                    UIManager.Instance.ClosePeekUI();
                }
            });
        }

        closeUIAction = CloseUI;
        InputManager.Instance.PlayerInput.actions["ESC"].performed += closeUIAction;

        preButton.onClick.AddListener(() =>
        {
            if (UIManager.Instance.ReturnPeekUI() as UIMenuPanel)
                UIManager.Instance.ClosePeekUI();
        });

        quitButton.onClick.AddListener(QuitGame);

        gotoStartUIButton.onClick.AddListener(() =>
        {
            if (RoomManager.Instance != null)
            {
                GameObject localPlayer = RoomManager.Instance.ReturnLocalPlayer();
                if (localPlayer != null)
                {
                    localPlayer.GetComponent<ParentPlayerController>().DeleteRuntimeData();
                }
            }

            PhotonNetwork.LeaveRoom();
        });

        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownValueChanged);
        windowDropdown.onValueChanged.AddListener(OnWindowDropdownValueChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderValueChanged);

        masterVolumeSlider.value = AudioManager.Instance.masterVolume;

        InitializeResolutionDropdown();
        InitializeWindowModeDropdown();
    }

    public void CloseUI(InputAction.CallbackContext ctx)
    {
        if (UIManager.Instance.ReturnPeekUI() as UIMenuPanel)
        {
            UIManager.Instance.ClosePeekUI();
        }
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby(); // �������� �κ� ������
        }
        else
        {
            SceneManager.LoadScene("Restart");
        }
    }

    public override void OnLeftLobby()
    {
        SceneManager.LoadScene("Restart");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // ������ ���� ����
#else
        Application.Quit();  // ����� ���� ����
#endif
    }

    /// ��� Guide / Option ��ư Ŭ�� �� ȣ��
    public void ShowLeftList(string listName)
    {
        optionList.SetActive(listName == "Option");
        guideList.SetActive(listName == "Guide");

        // ���� ����Ʈ �ٲ� ������ ���� ���� �г� ���� ����
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }

    /// ���� ���� �׸� Ŭ�� �� ȣ�� (ex: "Credit")
    public void ShowRightPanel(string panelName)
    {
        foreach (var panel in rightPanels)
        {
            panel.SetActive(panel.name == panelName);
        }
    }

    private void OnResolutionDropdownValueChanged(int index)
    {
        bool isFullScreen = Screen.fullScreen;
        switch (index)
        {
            case 0:
                Screen.SetResolution(1920, 1080, isFullScreen);
                DataManager.Instance.settingData.resolution = new Vector2Int(1920, 1080);
                DataManager.Instance.SaveSettingData();
                Debug.Log("1920, 1080");
                break;
            case 1:
                Screen.SetResolution(2560, 1440, isFullScreen);
                DataManager.Instance.settingData.resolution = new Vector2Int(2560, 1440);
                DataManager.Instance.SaveSettingData();
                Debug.Log("2560, 1440");
                break;
            case 2:
                Screen.SetResolution(3840, 2160, isFullScreen);
                DataManager.Instance.settingData.resolution = new Vector2Int(3840, 2160);
                DataManager.Instance.SaveSettingData();
                Debug.Log("3840, 2160");
                break;
        }
    }

    private void OnWindowDropdownValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Debug.Log("ExclusiveFullScreen");
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Debug.Log("Windowed");
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Debug.Log("FullScreenWindow");
                break;
        }

        DataManager.Instance.settingData.screenMode = Screen.fullScreenMode;
        DataManager.Instance.SaveSettingData();
    }

    private void InitializeWindowModeDropdown()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                windowDropdown.value = 0;
                break;
            case FullScreenMode.Windowed:
                windowDropdown.value = 1;
                break;
            case FullScreenMode.FullScreenWindow:
                windowDropdown.value = 2;
                break;
        }

        windowDropdown.RefreshShownValue();
    }

    private void InitializeResolutionDropdown()
    {
        var currentWidth = Screen.width;
        var currentHeight = Screen.height;

        for (int i = 0; i < resolutionDropdown.options.Count; i++)
        {
            var option = resolutionDropdown.options[i].text;
            if (option.Contains(currentWidth.ToString()) && option.Contains(currentHeight.ToString()))
            {
                resolutionDropdown.value = i;
                resolutionDropdown.RefreshShownValue(); // ǥ�� ��� ����
                break;
            }
        }
    }

    private void OnMasterVolumeSliderValueChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        DataManager.Instance.settingData.masterVolume = value;
        DataManager.Instance.SaveSettingData();
    }

    private void OnDisable()
    {
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.Player);

        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null && closeUIAction != null)
        {
            InputManager.Instance.PlayerInput.actions["ESC"].performed -= closeUIAction;
        }
    }
}
