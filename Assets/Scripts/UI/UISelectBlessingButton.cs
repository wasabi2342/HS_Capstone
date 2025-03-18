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

    private KeyValuePair<Skills, (Blessings, int)> thisBlessing;

    public void Init(KeyValuePair<Skills, (Blessings, int)> newBlessing)
    {
        thisBlessing = newBlessing;

        if (newBlessing.Value.Item2 > 1)
        {
            headerText.text = "���׷��̵�";
        }
        else
        {
            headerText.text = "�ű� ��ȣ";
        }
        // icon.sprite = ������ ��������Ʈ �޾ƿ���

        infoText.text = $"��� : {newBlessing.Key}\n��ȣ : {newBlessing.Value.Item1}���� �߰� �ؾ���";
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

    public KeyValuePair<Skills, (Blessings, int)> ReturnBlessing()
    {
        return thisBlessing;
    }
}
