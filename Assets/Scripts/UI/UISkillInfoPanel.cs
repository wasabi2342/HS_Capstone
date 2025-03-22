using System.Collections.Generic;
using UnityEngine;

public enum Blessing
{
    none,
    ���̸�,
    Ǫ��Ǫ��,
    �����ٽ�,
    �ƽ�Ÿ��Ʈ,
    �ٻ��
}

public enum InputKey
{
    mouseL,
    mouseR,
    space,
    shift,
    r,
    MAX
}

public class UISkillInfoPanel : UIBase
{
    [SerializeField]
    private UISkillDataSlot[] dataSlots = new UISkillDataSlot[5];

    public override void Init()
    {
        var blessings = RoomManager.Instance.ReturnLocalPlayer().GetComponentInChildren<PlayerBlessing>().ReturnBlessingDic();
        for (int i = 0; i < (int)Skills.Max; i++)
        {
            if (blessings[(Skills)i].level == 0)
            {
                dataSlots[i].Init("��ȣ ����", ((Skills)i).ToString());
            }
            else
            {
                dataSlots[i].Init(blessings[(Skills)i].blessing.ToString() + blessings[(Skills)i].level + "����", ((Skills)i).ToString());
            }
        }
    }
}
