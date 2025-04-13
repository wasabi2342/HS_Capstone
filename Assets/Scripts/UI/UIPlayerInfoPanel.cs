using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPlayerInfoPanel : UIBase
{
    [SerializeField]
    private Button selectCharacterButton;
    [SerializeField]
    private Text nicknameText;
    [SerializeField]
    private Image ReadyImage;

    public void Init(bool isReady, string nickname, string character, UnityAction action = null)
    {
        if (isReady)
        {
            ReadyImage.color = Color.green;

            if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
            {
                selectCharacterButton.interactable = true;
            }
            else
            {
                selectCharacterButton.interactable = false;
            }
        }
        else
        {
            ReadyImage.color = Color.red;
            selectCharacterButton.interactable = true;
        }

        nicknameText.text = nickname;

        selectCharacterButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprite/" + character);

        if (action != null)
        {
            selectCharacterButton.onClick.AddListener(action);
        }
    }
}
