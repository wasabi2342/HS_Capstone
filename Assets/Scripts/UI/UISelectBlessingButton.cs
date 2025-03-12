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

    private string[] skills = { "평타", "특수기", "대쉬", "스킬", "궁극기" };

    public void Init(Skills key, Blessings blessing, int level)
    {
        if(level > 1)
        {
            headerText.text = "업그레이드";
        }
        else
        {
            headerText.text = "신규 가호";
        }
        // icon.sprite = 아이콘 스프라이트 받아오기

        infoText.text = $"기능 : {skills[(int)key]}\n가호 : {blessing}설명 추가 해야함";
    }
    
    public void SetEnabled()
    {
        headerText.enabled = true;
        icon.enabled = true;
        infoText.enabled = true;
    }
}
