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

    private Dictionary<int, UIPartyHPContent> contentPairs = new Dictionary<int, UIPartyHPContent>();

    private void Start()
    {
        InputManager.Instance.PlayerInput.actions["OpenBlessingInfo"].performed += ctx => OpenBlessingInfoPanel(ctx);
        RoomManager.Instance.UIUpdate += Init;
        Init();
    }

    public override void Init() // 선택된 캐릭터 정보를 받아와 이미지 갱신
    {
        foreach (var icon in uISkillIcons)
        {

        }

        foreach (var keyValuePair in RoomManager.Instance.players)
        {
            Debug.Log("RoomManager.Instance.players.Count : " + RoomManager.Instance.players.Count);
            if (keyValuePair.Key != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                UIPartyHPContent content = Instantiate(partyHPContent, partyHPParent);
                content.Init(GetNicknameByActNum(keyValuePair.Key));
                // UI 연결 추가하기
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null)
                {
                    playerController.OnHealthChanged.RemoveAllListeners();
                    playerController.OnHealthChanged.AddListener(content.UpdateHPImage);
                }
                contentPairs[keyValuePair.Key] = content;
            }
            // UI 연결 추가하기
            else
            {
                ParentPlayerController playerController = keyValuePair.Value.GetComponent<ParentPlayerController>();
                if (playerController != null) // 쿨타임 이벤트도 연결 하기
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
                    //playerController
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
        InputManager.Instance.ChangeDefaultMap("UI");
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

    public void UpdateMouseLeftStack(float stack)
    {
        mouseLStackText.text = ((int)stack).ToString();
    }

    private void UpdateHPImage(float fillAmount)
    {
        hpImage.fillAmount = fillAmount;
    }
}
