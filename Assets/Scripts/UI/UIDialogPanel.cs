using UnityEngine;
using UnityEngine.UI;

public class UIDialogPanel : UIBase
{
    [SerializeField]
    private Text infoText;
    [SerializeField]
    private Button confirmButton;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        confirmButton.onClick.AddListener(OnClickedConfirmButton);
    }

    private void OnClickedConfirmButton()
    {
        UIManager.Instance.ClosePeekUI();
    }

    public void SetInfoText(string info)
    {
        infoText.text = info;
    }
}
