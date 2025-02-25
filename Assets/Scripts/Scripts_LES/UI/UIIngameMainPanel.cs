using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        Init();
    }

    public override void Init() // 선택된 캐릭터 정보를 받아와 이미지 갱신
    {
        foreach (var icon in uISkillIcons)
        {

        }
    }

    public void UpdateIconOutline(Color color, UIIcon icon)
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
