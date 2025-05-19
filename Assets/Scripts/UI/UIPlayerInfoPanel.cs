using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPlayerInfoPanel : UIBase
{
    //[SerializeField]
    //private Button selectCharacterButton;
    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private TextMeshProUGUI nicknameText;
    [SerializeField]
    private Image ReadyImage;

    public void Init(bool isReady, string nickname, string character, UnityAction action = null)
    {
        if (isReady)
        {
            ReadyImage.color = Color.green;
        }
        else
        {
            ReadyImage.color = Color.red;
        }

        nicknameText.text = nickname;

        characterImage.sprite = Resources.Load<Sprite>("Sprite/" + character);
    }
}
