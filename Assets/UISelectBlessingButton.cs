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

    private string[] skills = { "��Ÿ", "Ư����", "�뽬", "��ų", "�ñر�" };

    public void Init(Skills key, Blessings blessing, int level)
    {
        if(level > 1)
        {
            headerText.text = "���׷��̵�";
        }
        else
        {
            headerText.text = "�ű� ��ȣ";
        }
        // icon.sprite = ������ ��������Ʈ �޾ƿ���

        infoText.text = $"��� : {skills[(int)key]}\n��ȣ : {blessing}���� �߰� �ؾ���";
    }
    
    public void SetEnabled()
    {
        headerText.enabled = true;
        icon.enabled = true;
        infoText.enabled = true;
    }
}
