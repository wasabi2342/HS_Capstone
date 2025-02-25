using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Skill
{

}

public class UIIngameMainPanel : UIBase
{
    [SerializeField]
    private List<UISkillIcon> uISkillIcons = new List<UISkillIcon>();
    [SerializeField]
    private Image hpImage;

    private void Start()
    {
        Init();
    }

    public override void Init() // 선택된 캐릭터 정보를 받아와 이미지 갱신
    {
        foreach (var icon in uISkillIcons)
        {
            icon.Init(Color.green); // 스킬 아이콘에 가호를 통한 테두리 색상등 정보를 넘겨주기
        }
    }

    public void UpdateHPImage(float fillAmount)
    {
        hpImage.fillAmount = fillAmount;
    }
}
