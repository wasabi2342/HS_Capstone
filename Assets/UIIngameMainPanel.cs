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

    public override void Init() // ���õ� ĳ���� ������ �޾ƿ� �̹��� ����
    {
        foreach (var icon in uISkillIcons)
        {
            icon.Init(Color.green); // ��ų �����ܿ� ��ȣ�� ���� �׵θ� ����� ������ �Ѱ��ֱ�
        }
    }

    public void UpdateHPImage(float fillAmount)
    {
        hpImage.fillAmount = fillAmount;
    }
}
