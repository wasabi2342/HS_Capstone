using UnityEngine;
using UnityEngine.UI;

public class UISettingPanel : UIBase
{
    [SerializeField]
    private Button changeNicknameButton;
    [SerializeField]
    private Button confirmButton;

    public override void Init()
    {
        changeNicknameButton.onClick.AddListener(() => UIManager.Instance.OpenPopupPanel<UISetNicknamePanel>());
        confirmButton.onClick.AddListener(() => UIManager.Instance.ClosePeekUI());
    }

    void Start()
    {
        Init();
    }
}
