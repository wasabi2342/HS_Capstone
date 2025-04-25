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

    private string[] skills = { "��Ÿ", "Ư����", "�뽬", "��ų", "�ñر�" };

    private SkillWithLevel thisBlessing;

    public void Init(SkillWithLevel newBlessing)
    {
        thisBlessing = newBlessing;

        if (newBlessing.level > 1)
        {
            headerText.text = "���׷��̵�";
        }
        else
        {
            headerText.text = "�ű� ��ȣ";
        }
        //icon.sprite = Resources.Load<Sprite>($"Blessing/Back/{(Blessings)newBlessing.skillData.Devil}"); ���߿� �ٲٱ�

        icon.sprite = Resources.Load<Sprite>("Blessing/Back/Crocell");  // �ӽ� ī��
        infoText.text = $"{newBlessing.skillData.Blessing_name}\n{newBlessing.skillData.Bless_Discript}";
    }
    
    public void SetEnabled()
    {
        headerText.enabled = true;
        //icon.enabled = true;
        infoText.enabled = true;
        //icon.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)newBlessing.skillData.Devil}"); ���߿� �ٲٱ� �ε����� �ٲ� ����
        icon.sprite = Resources.Load<Sprite>("Blessing/Front/Crocell");  // �ӽ� ī��
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
