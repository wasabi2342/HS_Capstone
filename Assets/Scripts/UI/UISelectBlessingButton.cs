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

    private KeyValuePair<Skills, BlessingInfo> thisBlessing;

    public void Init(KeyValuePair<Skills, BlessingInfo> newBlessing)
    {
        thisBlessing = newBlessing;

        if (newBlessing.Value.level > 1)
        {
            headerText.text = "업그레이드";
        }
        else
        {
            headerText.text = "신규 가호";
        }
        // icon.sprite = 아이콘 스프라이트 받아오기

        infoText.text = $"기능 : {newBlessing.Key}\n가호 : {newBlessing.Value.blessing}설명 추가 해야함";
    }
    
    public void SetEnabled()
    {
        headerText.enabled = true;
        icon.enabled = true;
        infoText.enabled = true;
    }

    public void OutlineEnabled(bool value)
    {
        outline.enabled = value;
    }

    public KeyValuePair<Skills, BlessingInfo> ReturnBlessing()
    {
        return thisBlessing;
    }
}
