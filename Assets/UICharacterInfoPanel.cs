using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UICharacterInfoPanel : UIBase
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text infoText;
    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private Button preButton;
    [SerializeField]
    private Button selectButton;

    public void Init(CharacterStats stats)
    {
        nameText.text = stats.name;
        infoText.text = $"�ִ�ü�� : {stats.maxHP}\n�������ݷ� : {stats.attackPower}\n�������ݷ� : {stats.abilityPower}\n���ݼӵ� : {stats.attackSpeed}\n�̵��ӵ� : {stats.moveSpeed}";
        characterImage.sprite = Resources.Load<Sprite>("Sprite/" + stats.name);

        preButton.onClick.AddListener(OnClickedPreButton);
        selectButton.onClick.AddListener( () => OnClickedSelectButton(stats.name));
    }

    private void OnClickedPreButton()
    {
        UIManager.Instance.ClosePeekUI();
    }

    private void OnClickedSelectButton(string name)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "SelectCharacter", name }
            });
        //UIManager.Instance.CloseAllAndOpen<UIRoomPanel>();
    }
}
