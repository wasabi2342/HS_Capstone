using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
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

    public void Init(CharacterStats stats, UnityAction action)
    {
        nameText.text = stats.name;
        infoText.text = $"최대체력 : {stats.maxHP}\n물리공격력 : {stats.attackPower}\n마법공격력 : {stats.abilityPower}\n공격속도 : {stats.attackSpeed}\n이동속도 : {stats.moveSpeed}";
        characterImage.sprite = Resources.Load<Sprite>("Sprite/" + stats.name);

        preButton.onClick.AddListener(OnClickedPreButton);
        selectButton.onClick.AddListener(() => OnClickedSelectButton(stats.name, action));
    }

    private void OnClickedPreButton()
    {
        UIManager.Instance.ClosePeekUI();
    }

    private void OnClickedSelectButton(string name, UnityAction action)
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "SelectCharacter", name }
            });
        }
        else
        {
            PlayerPrefs.SetString("SelectCharacter", name);
            PlayerPrefs.Save();

            if (RoomManager.Instance == null)
            {
                UIRoomPanel roomPanel = FindObjectOfType<UIRoomPanel>();
                roomPanel?.UpdateMyCharacterImage(name);
            }
        }
        action?.Invoke();
    }
}
