using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectBlessingButton : UIBase
{
    [SerializeField]
    private Text headerText;
    [SerializeField]
    private Image cardBG;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Text infoText;
    [SerializeField]
    private Text blessingNameText;
    [SerializeField]
    private Text devilNameText;
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

        icon.enabled = false;
        headerText.enabled = false;
        infoText.enabled = false;
        blessingNameText.enabled = false;
        //devilNameText.enabled = false;

        cardBG.sprite = Resources.Load<Sprite>("Blessing/Back/Crocell");  // 임시 카드
        blessingNameText.text = $"{newBlessing.skillData.Blessing_name}";
        infoText.text = $"{newBlessing.skillData.Bless_Discript}";
        // devilNameText.text = $"{(Blessings)newBlessing.skillData.Devil}"; 나중에 쓸 예정
    }

    public void SetEnabled()
    {
        cardBG.material = null;

        headerText.enabled = true;
        infoText.enabled = true;
        blessingNameText.enabled = true;
        icon.enabled = true;
        //devilNameText.enabled = true; 나중에 주석제거

        cardBG.sprite = Resources.Load<Sprite>("Blessing/Front/Front");  // 임시 카드
        cardBG.SetNativeSize();

        Sprite sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)thisBlessing.skillData.ID}"); //나중에 바꾸기 인덱스로 바뀔 수도
        if (sprite != null)
            icon.sprite = sprite;
        else
            icon.sprite = Resources.Load<Sprite>("Blessing/Front/card_image");  // 이미지 이름 나중에 맞는 가호로 바꾸기

        //icon.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)thisBlessing.skillData.ID}"); //나중에 바꾸기 인덱스로 바뀔 수도

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
