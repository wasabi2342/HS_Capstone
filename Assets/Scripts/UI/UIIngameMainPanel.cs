using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
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
    private Image shieldImage;
    [SerializeField]
    private Text mouseLStackText;
    [SerializeField]
    private TextMeshProUGUI shieldText;
    [SerializeField]
    private RectTransform partyHPParent;
    [SerializeField]
    private UIPartyHPContent partyHPContent;

    private Dictionary<int, UIPartyHPContent> contentPairs = new Dictionary<int, UIPartyHPContent>();

    private void Start()
    {
        InputManager.Instance.PlayerInput.actions["OpenBlessingInfo"].performed += ctx => OpenBlessingInfoPanel(ctx);
        RoomManager.Instance.UIUpdate += AddPartyPlayerHPbar;
        Init();
    }


    [SerializeField] private Image stunOverlay;
    [SerializeField] private Image stunSlider;

    public override void Init() // ���õ� ĳ���� ������ �޾ƿ� �̹��� ����
    {
        foreach (var icon in uISkillIcons)
        {

        }

        ParentPlayerController playerController = RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>();
        if (playerController != null) // ��Ÿ�� �̺�Ʈ�� ���� �ϱ�
        {
            playerController.OnHealthChanged.RemoveAllListeners();
            playerController.OnHealthChanged.AddListener(UpdateHPImage);
            playerController.ShiftCoolDownUpdate.RemoveAllListeners();
            playerController.ShiftCoolDownUpdate.AddListener(uISkillIcons[(int)UIIcon.shift].StartUpdateSkillCooldown);
            playerController.UltimateCoolDownUpdate.RemoveAllListeners();
            playerController.UltimateCoolDownUpdate.AddListener(uISkillIcons[(int)UIIcon.r].StartUpdateSkillCooldown);
            playerController.MouseRightSkillCoolDownUpdate.RemoveAllListeners();
            playerController.MouseRightSkillCoolDownUpdate.AddListener(uISkillIcons[(int)UIIcon.mouseR].StartUpdateSkillCooldown);
            playerController.OnDashCooldownUpdate.RemoveAllListeners();
            playerController.OnDashCooldownUpdate.AddListener(uISkillIcons[(int)UIIcon.space].StartUpdateSkillCooldown);
            playerController.AttackStackUpdate.RemoveAllListeners();
            playerController.AttackStackUpdate.AddListener(UpdateMouseLeftStack);
            playerController.SkillOutlineUpdate.RemoveAllListeners();
            playerController.SkillOutlineUpdate.AddListener(UpdateIconOutline);
            playerController.hpBar = hpImage;
            playerController.stunOverlay = stunOverlay;
            playerController.stunSlider = stunSlider;
            playerController.ShieldUpdate.RemoveAllListeners();
            playerController.ShieldUpdate.AddListener(UpdateShieldImage);
        }
    }

    public void AddPartyPlayerHPbar(int actnum, GameObject newPlayer)
    {
        UIPartyHPContent content = Instantiate(partyHPContent, partyHPParent);
        content.Init(GetNicknameByActNum(actnum));
        ParentPlayerController playerController = newPlayer.GetComponent<ParentPlayerController>();
        if (playerController != null)
        {
            playerController.OnHealthChanged.RemoveAllListeners();
            playerController.OnHealthChanged.AddListener(content.UpdateHPImage);
        }
        contentPairs[actnum] = content;
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

        playerController = RoomManager.Instance.players[PhotonNetwork.LocalPlayer.ActorNumber].GetComponent<ParentPlayerController>();
        if (playerController != null)
        {
            playerController.OnHealthChanged.RemoveListener(UpdateHPImage);
            playerController.ShiftCoolDownUpdate.RemoveListener(uISkillIcons[(int)UIIcon.shift].StartUpdateSkillCooldown);
            playerController.UltimateCoolDownUpdate.RemoveListener(uISkillIcons[(int)UIIcon.r].StartUpdateSkillCooldown);
            playerController.MouseRightSkillCoolDownUpdate.RemoveListener(uISkillIcons[(int)UIIcon.mouseR].StartUpdateSkillCooldown);
            playerController.OnDashCooldownUpdate.RemoveListener(uISkillIcons[(int)UIIcon.space].StartUpdateSkillCooldown);
            playerController.AttackStackUpdate.RemoveListener(UpdateMouseLeftStack);
            playerController.SkillOutlineUpdate.RemoveAllListeners();
        }
    }

    public string GetNicknameByActNum(int actNum)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actNum)
            {
                return player.NickName;
            }
        }
        return "Unknown";
    }

    public void OpenBlessingInfoPanel(InputAction.CallbackContext ctx)
    {
        UIManager.Instance.OpenPopupPanel<UISkillInfoPanel>();
        InputManager.Instance.ChangeDefaultMap(InputDefaultMap.UI);
    }

    public void UpdateIconOutline(UIIcon icon, Color color)
    {
        if ((int)icon > 4)
            return;
        uISkillIcons[(int)icon].SetOutlineColor(color);
    }

    public void UpdateMouseLeftStack(float stack)
    {
        mouseLStackText.text = ((int)stack).ToString();
    }

    private void UpdateHPImage(float fillAmount)
    {
        hpImage.fillAmount = fillAmount;
    }
    private void UpdateShieldImage(float shieldAmount, float maxShield)
    {
        shieldImage.fillAmount = shieldAmount / maxShield;
        shieldText.text = $"{shieldAmount}/{maxShield}";
    }
}
