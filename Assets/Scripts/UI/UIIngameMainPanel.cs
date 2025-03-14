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

    public override void Init() // 선택된 캐릭터 정보를 받아와 이미지 갱신
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
                // UI 연결 추가하기
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null)
                {
                    playerController.OnHealthChanged.AddListener(content.UpdateHPImage);
                }
                contentPairs.Add(keyValuePair.Key, content);
            }
            // UI 연결 추가하기
            else
            {
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null) // 쿨타임 이벤트도 연결 하기
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
