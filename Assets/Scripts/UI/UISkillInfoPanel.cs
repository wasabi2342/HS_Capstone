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
        var blessings = RoomManager.Instance.ReturnLocalPlayer().GetComponentInChildren<PlayerBlessing>().ReturnBlessingDic();
        for (int i = 0; i < (int)Skills.Max; i++)
        {
            if (blessings[(Skills)i].level == 0)
            {
                dataSlots[i].Init("가호 없음", ((Skills)i).ToString());
            }
            else
            {
                dataSlots[i].Init(blessings[(Skills)i].blessing.ToString() + blessings[(Skills)i].level + "레벨", ((Skills)i).ToString());
            }
        }
    }
}
