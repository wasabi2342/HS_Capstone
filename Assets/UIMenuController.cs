using Photon.Pun;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuPanel : UIBase
{
    [Header("왼쪽 스크롤 뷰 그룹")]
    public GameObject optionList;
    public GameObject guideList;

    [Header("우측 설명 패널들")]
    public GameObject[] rightPanels; // Credit, GraphicOption, Character 등

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
        // 시작할 때 모든 RightPanel 내용 끄기
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
            PhotonNetwork.LeaveLobby(); // 다음으로 로비 나가기
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
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터 실행 중지
#else
        Application.Quit();  // 빌드된 게임 종료
#endif
    }

    /// 상단 Guide / Option 버튼 클릭 시 호출
    public void ShowLeftList(string listName)
    {
        optionList.SetActive(listName == "Option");
        guideList.SetActive(listName == "Guide");

        // 하위 리스트 바꿀 때마다 우측 설명 패널 전부 끄기
        foreach (var panel in rightPanels)
        {
            panel.SetActive(false);
        }
    }

    /// 좌측 세부 항목 클릭 시 호출 (ex: "Credit")
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
                resolutionDropdown.RefreshShownValue(); // 표시 즉시 갱신
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
