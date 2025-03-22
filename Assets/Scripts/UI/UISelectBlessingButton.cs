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

    private KeyValuePair<Skills, BlessingInfo> thisBlessing;

    public void Init(KeyValuePair<Skills, BlessingInfo> newBlessing)
    {
        thisBlessing = newBlessing;

        if (newBlessing.Value.level > 1)
        {
            headerText.text = "���׷��̵�";
        }
        else
        {
            headerText.text = "�ű� ��ȣ";
        }
        // icon.sprite = ������ ��������Ʈ �޾ƿ���

        infoText.text = $"��� : {newBlessing.Key}\n��ȣ : {newBlessing.Value.blessing}���� �߰� �ؾ���";
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
