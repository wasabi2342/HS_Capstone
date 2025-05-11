using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
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
    [SerializeField]
    private TextMeshProUGUI etherText;
    [SerializeField]
    private TextMeshProUGUI goldText;
    [SerializeField]
    private List<Image> playerIconList = new List<Image>();
    [SerializeField]
    private Image hitOverlay;
    [SerializeField] 
    private float hitOverlayDuration = 0.5f;
    
    private Coroutine hitOverlayCoroutine;
    
    private Dictionary<int, UIPartyHPContent> contentPairs = new Dictionary<int, UIPartyHPContent>();

    private System.Action<InputAction.CallbackContext> openBlessingInfoAction;

    private void Start()
    {
        StartCoroutine(Co_Start());
    }

    IEnumerator Co_Start()
    {
        yield return null;

        openBlessingInfoAction = OpenBlessingInfoPanel;
        InputManager.Instance.PlayerInput.actions["OpenBlessingInfo"].performed += openBlessingInfoAction;

        RoomManager.Instance.UIUpdate += AddPartyPlayerHPbar;
        Init();
    }

    [SerializeField] private Image stunOverlay;
    [SerializeField] private Image stunSlider;

    public override void Init() // 선택된 캐릭터 정보를 받아와 이미지 갱신
    {
        string playerCharacter = RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>().ReturnCharacterName();
        for (int i = 0; i < uISkillIcons.Count; i++)
        {
            uISkillIcons[i].SetImage(playerCharacter, (Skills)i, Blessings.None);
        }

        ParentPlayerController playerController = RoomManager.Instance.ReturnLocalPlayer().GetComponent<ParentPlayerController>();
        if (playerController != null) // 쿨타임 이벤트도 연결 하기
        {
            playerController.OnHealthChanged.RemoveAllListeners();
            playerController.OnHealthChanged.AddListener(UpdateHPImage);
            playerController.OnAttackCooldownUpdate.RemoveAllListeners();
            playerController.OnAttackCooldownUpdate.AddListener(uISkillIcons[(int)UIIcon.mouseL].StartUpdateSkillCooldown);
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
            playerController.OnHitEvent.RemoveAllListeners();
            playerController.OnHitEvent.AddListener(OnHitOverlay);
            playerController.UpdateHP();
        }

        if (Enum.TryParse<Characters>(playerController.ReturnCharacterName(), out var character))
        {
            int characterInt = (int)character;
            playerIconList[characterInt].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("변환 실패!");
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
            playerController.UpdateHP();
        }
        contentPairs[actnum] = content;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.PlayerInput != null)
        {
            InputManager.Instance.PlayerInput.actions["OpenBlessingInfo"].performed -= openBlessingInfoAction;
        }

        ParentPlayerController playerController;

        foreach (var keyValuePair in contentPairs)
        {
            if (RoomManager.Instance.players.ContainsKey(keyValuePair.Key))
            {
                playerController = RoomManager.Instance.players[keyValuePair.Key].GetComponent<ParentPlayerController>();
                if (playerController != null)
                {
                    playerController.OnHealthChanged.RemoveListener(keyValuePair.Value.UpdateHPImage);
                }
            }
            else
            {
                Debug.LogWarning($"Key {keyValuePair.Key} not found in players dictionary.");
            }
        }

        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (RoomManager.Instance.players.ContainsKey(localActorNumber))
        {
            playerController = RoomManager.Instance.players[localActorNumber].GetComponent<ParentPlayerController>();
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
        else
        {
            Debug.LogWarning($"Local player with ActorNumber {localActorNumber} not found in players dictionary.");
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
        if (UIManager.Instance.ReturnPeekUI() as UIMenuPanel)
            return;
        UIManager.Instance.OpenPopupPanelInOverlayCanvas<UISkillInfoPanel>();
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

    public void OnHitOverlay()
    {
        // 이전 코루틴이 실행 중이면 중단
        if (hitOverlayCoroutine != null)
        {
            StopCoroutine(hitOverlayCoroutine);
        }

        // 새 코루틴 시작
        hitOverlayCoroutine = StartCoroutine(HitOverlayFade());
    }

    private IEnumerator HitOverlayFade()
    {
        // 이미지 활성화 및 알파 1로 설정
        hitOverlay.gameObject.SetActive(true);
        Color color = hitOverlay.color;
        color.a = 1f;
        hitOverlay.color = color;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            hitOverlay.color = color;
            yield return new WaitForFixedUpdate();
        }

        // 완전히 투명해진 후 비활성화
        color.a = 0f;
        hitOverlay.color = color;
        hitOverlay.gameObject.SetActive(false);
        hitOverlayCoroutine = null;
    }
}
