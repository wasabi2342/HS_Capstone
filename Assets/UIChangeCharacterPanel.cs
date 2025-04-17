using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChangeCharacterPanel : UIBase
{
    [SerializeField]
    private List<CharacterStats> characterDatas = new List<CharacterStats>();
    [SerializeField]
    private Button characterButton;
    [SerializeField]
    private Button cancelButton;
    [SerializeField]
    private RectTransform content;

    private GameObject player;
    private int selectedCharacterIndex;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        for (int i = 0; i < characterDatas.Count; i++)
        {
            int index = i;
            Button button = Instantiate(characterButton);
            button.transform.SetParent(content.transform, false);
            button.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprite/" + characterDatas[index].name);
            button.onClick.AddListener(() =>
            {
                selectedCharacterIndex = index;
                UIManager.Instance.OpenPopupPanel<UICharacterInfoPanel>().Init(characterDatas[index], SelcetCharacter);
            });
        }
        cancelButton.onClick.AddListener(OnClickedCancelButton);
    }

    public void GetCharacter(GameObject player)
    {
        this.player = player;
    }

    public void SelcetCharacter()
    {
        if (PhotonNetwork.InRoom)
        {
            if (!(characterDatas[selectedCharacterIndex].name == player.GetComponent<ParentPlayerController>().ReturnNmae()))
            {
                RoomManager.Instance.CreateCharacter(characterDatas[selectedCharacterIndex].name, player.transform.position, player.transform.rotation, true);
                PhotonNetwork.Destroy(player);
            }
            UIManager.Instance.ClosePeekUI();
        }
        else
        {
            if (!(characterDatas[selectedCharacterIndex].name == player.GetComponent<ParentPlayerController>().ReturnNmae()))
            {
                RoomManager.Instance.CreateCharacter(characterDatas[selectedCharacterIndex].name, player.transform.position, player.transform.rotation, true);
                Destroy(player);
            }
        }

        UIManager.Instance.ClosePeekUI();
    }

    public void OnClickedCancelButton()
    {
        UIManager.Instance.ClosePeekUI();
    }
}
