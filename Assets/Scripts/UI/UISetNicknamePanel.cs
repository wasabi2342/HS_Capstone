using UnityEngine;
using UnityEngine.UI;

public class UISetNicknamePanel : UIBase
{
    [SerializeField]
    private InputField inputField;
    [SerializeField]
    private Button button;

    public override void Init()
    {
        button.onClick.AddListener(OnClickedButton);
    }

    void Start()
    {
        Init();
    }

    private void OnClickedButton()
    {
        if (inputField.text != "")
        {
            PhotonNetworkManager.Instance.SetNickname(inputField.text);
            PlayerPrefs.SetString("Nickname", inputField.text);
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            UIManager.Instance.OpenPopupPanelInOverlayCanvas<UIDialogPanel>().SetInfoText("닉네임을 입력해 주세요");
        }
    }

}
