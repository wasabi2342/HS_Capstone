using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBlessingButton : UIBase
{
    [SerializeField]
    private Text headerText;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Text infoText;
    [SerializeField]
    private UnityEngine.UI.Outline outline;

    private string[] skills = { "평타", "특수기", "대쉬", "스킬", "궁극기" };

    private SkillWithLevel thisBlessing;

    public void Init(SkillWithLevel newBlessing)
    {
        thisBlessing = newBlessing;

        if (newBlessing.level > 1)
        {
            headerText.text = "업그레이드";
        }
        else
        {
            headerText.text = "신규 가호";
        }
        //icon.sprite = Resources.Load<Sprite>($"Blessing/Back/{(Blessings)newBlessing.skillData.Devil}"); 나중에 바꾸기

        icon.sprite = Resources.Load<Sprite>("Blessing/Back/Crocell");  // 임시 카드
        infoText.text = $"{newBlessing.skillData.Blessing_name}\n{newBlessing.skillData.Bless_Discript}";
    }
    
    public void SetEnabled()
    {
        headerText.enabled = true;
        //icon.enabled = true;
        infoText.enabled = true;
        //icon.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)newBlessing.skillData.Devil}"); 나중에 바꾸기 인덱스로 바뀔 수도
        icon.sprite = Resources.Load<Sprite>("Blessing/Front/Crocell");  // 임시 카드
    }

    public void OutlineEnabled(bool value)
    {
        outline.enabled = value;
    }

    public SkillWithLevel ReturnBlessing()
    {
        return thisBlessing;
    }
}
