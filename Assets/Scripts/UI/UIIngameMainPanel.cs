using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum UIIcon
{
    mouseL,
    mouseR,
    space,
    shift,
    r,
    hpBar,
    mouseLStack
}

public class UIIngameMainPanel : UIBase
{
    [SerializeField]
    private List<UISkillIcon> uISkillIcons = new List<UISkillIcon>();
    [SerializeField]
    private Image hpImage;
    [SerializeField]
    private Text mouseLStackText;
    [SerializeField]
    private RectTransform partyHPParent;
    [SerializeField]
    private UIPartyHPContent partyHPContent;

    private Dictionary<string, UIPartyHPContent> contentPairs = new Dictionary<string, UIPartyHPContent>();

    private void Start()
    {
        InputManager.Instance.PlayerInput.actions["OpenBlessingInfo"].performed += ctx => OpenBlessingInfoPanel(ctx);
        Init();
    }

    public override void Init() // ���õ� ĳ���� ������ �޾ƿ� �̹��� ����
    {
        foreach (var icon in uISkillIcons)
        {

        }

        foreach (var keyValuePair in RoomManager.Instance.players)
        {
            if (keyValuePair.Key != PhotonNetwork.LocalPlayer.UserId)
            {
                UIPartyHPContent content = Instantiate(partyHPContent, partyHPParent);
                content.Init(GetNicknameByUserId(keyValuePair.Key));
                // UI ���� �߰��ϱ�
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null)
                {
                    playerController.OnHealthChanged.AddListener(content.UpdateHPImage);
                }
                contentPairs.Add(keyValuePair.Key, content);
            }
            // UI ���� �߰��ϱ�
            else
            {
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null) // ��Ÿ�� �̺�Ʈ�� ���� �ϱ�
                {
                    playerController.OnHealthChanged.AddListener(UpdateHPImage);
                }
            }
        }
    }

    private void OnDisable()
    {
        ParentPlayerController playerController;

        foreach (var keyValuePair in contentPairs)
        {
            playerController = RoomManager.Instance.players[keyValuePair.Key].GetComponent<ParentPlayerController>();
            if (playerController != null)
            {
                playerController.OnHealthChanged.RemoveListener(keyValuePair.Value.UpdateHPImage);
            }
        }

        playerController = RoomManager.Instance.players[PhotonNetwork.LocalPlayer.UserId].GetComponent<ParentPlayerController>();
        if (playerController != null)
        {
            playerController.OnHealthChanged.RemoveListener(UpdateHPImage);
        }
    }

    public string GetNicknameByUserId(string userId)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == userId)
            {
                return player.NickName;
            }
        }
        return "Unknown";
    }

    public void OpenBlessingInfoPanel(InputAction.CallbackContext ctx)
    {
        UIManager.Instance.OpenPopupPanel<UISkillInfoPanel>();
    }

    public void UpdateIconOutline(UIIcon icon, Color color)
    {
        if ((int)icon > 4)
            return;
        uISkillIcons[(int)icon].SetOutlineColor(color);
    }

    public void UpdateUI(UIIcon icon, float value)
    {
        if ((int)icon == 6)
        {
            mouseLStackText.text = ((int)value).ToString();
        }
        else if ((int)icon == 5)
        {
            UpdateHPImage(value);
        }
        else
        {
            uISkillIcons[(int)icon].StartUpdateSkillCooldown(value);
        }
    }

    private void UpdateHPImage(float fillAmount)
    {
        hpImage.fillAmount = fillAmount;
    }
}
