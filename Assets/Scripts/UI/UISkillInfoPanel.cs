using System.Collections.Generic;
using UnityEngine;

public enum Blessing
{
    none,
    파이몬,
    푸르푸르,
    마르바스,
    아스타로트,
    바사고
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
                dataSlots[i].Init("가호 없음", ((InputKey)i).ToString());
            }
            else
            {
                dataSlots[i].Init(blessings[(InputKey)i].blessing.ToString() + blessings[(InputKey)i].level + "레벨", ((InputKey)i).ToString());
            }
        }
    }
}
