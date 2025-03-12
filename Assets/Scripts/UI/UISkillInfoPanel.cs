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
        var blessings = RoomManager.Instance.localPlayer.blessings;
        for (int i = 0; i < (int)InputKey.MAX; i++)
        {
            if (blessings[(InputKey)i].blessing == Blessing.none)
            {
                dataSlots[i].Init("��ȣ ����", ((InputKey)i).ToString());
            }
            else
            {
                dataSlots[i].Init(blessings[(InputKey)i].blessing.ToString() + blessings[(InputKey)i].level + "����", ((InputKey)i).ToString());
            }
        }
    }
}
