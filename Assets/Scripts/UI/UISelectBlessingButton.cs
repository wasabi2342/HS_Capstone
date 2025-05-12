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

        icon.enabled = false;
        headerText.enabled = false;
        infoText.enabled = false;
        blessingNameText.enabled = false;
        //devilNameText.enabled = false;

        cardBG.sprite = Resources.Load<Sprite>("Blessing/Back/Crocell");  // �ӽ� ī��
        blessingNameText.text = $"{newBlessing.skillData.Blessing_name}";
        infoText.text = $"{newBlessing.skillData.Bless_Discript}";
        // devilNameText.text = $"{(Blessings)newBlessing.skillData.Devil}"; ���߿� �� ����
    }

    public void SetEnabled()
    {
        cardBG.material = null;

        headerText.enabled = true;
        infoText.enabled = true;
        blessingNameText.enabled = true;
        icon.enabled = true;
        //devilNameText.enabled = true; ���߿� �ּ�����

        cardBG.sprite = Resources.Load<Sprite>("Blessing/Front/Front");  // �ӽ� ī��
        cardBG.SetNativeSize();

        Sprite sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)thisBlessing.skillData.ID}"); //���߿� �ٲٱ� �ε����� �ٲ� ����
        if (sprite != null)
            icon.sprite = sprite;
        else
            icon.sprite = Resources.Load<Sprite>("Blessing/Front/card_image");  // �̹��� �̸� ���߿� �´� ��ȣ�� �ٲٱ�

        //icon.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)thisBlessing.skillData.ID}"); //���߿� �ٲٱ� �ε����� �ٲ� ����

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
