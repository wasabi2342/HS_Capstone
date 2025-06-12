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
    private TextMeshProUGUI readyText;
    [SerializeField]
    private TextMeshProUGUI hostText;

    public void Init(bool isReady, string nickname, string character, bool isMaster, UnityAction action = null)
    {
        if (isReady)
        {
            if (isMaster)
            {
                hostText.gameObject.SetActive(true);
            }
            else
            {
                readyText.gameObject.SetActive(true);
            }
        }
        else
        {
            readyText.gameObject.SetActive(false);
        }

        nicknameText.text = nickname;

        characterImage.sprite = Resources.Load<Sprite>("Sprite/" + character);
    }
}
