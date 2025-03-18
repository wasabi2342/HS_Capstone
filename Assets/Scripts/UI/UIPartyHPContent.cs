using UnityEngine;
using UnityEngine.UI;

public class UIPartyHPContent : UIBase
{
    [SerializeField]
    private Image hpImage;
    [SerializeField]
    private Text nicknameText;

    public void Init(string nickname)
    {
        nicknameText.text = nickname;
    }

    public void UpdateHPImage(float value)
    {
        hpImage.fillAmount = value;
    }
}
