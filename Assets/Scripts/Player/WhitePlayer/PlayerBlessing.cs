using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R, Max }
public enum Blessings { None, Crocell, Gremory, Paymon, Max }

[Serializable]
public class SkillWithLevel
{
    public SkillData skillData;
    public int level;

    public SkillWithLevel(SkillData skillData, int level)
    {
        this.skillData = skillData;
        this.level = level;
    }
}

public class PlayerBlessing : MonoBehaviourPun
{
    [SerializeField]
    private ParentPlayerController playerController;
    private Animator animator;
    //private Dictionary<Skills, SkillWithLevel> playerBlessingDic = new Dictionary<Skills, SkillWithLevel>();

    public List<BaseSpecialEffect> specialEffectList;

    Dictionary<Blessings, Color> BlessingColor = new Dictionary<Blessings, Color>
{
    { Blessings.None, Color.clear },
    { Blessings.Crocell, Color.blue },
    { Blessings.Gremory, Color.magenta },
    { Blessings.Paymon, Color.red }
};

    private void Awake()
    {
        playerController = GetComponent<ParentPlayerController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        SkillWithLevel[] blessings = playerController.ReturnBlessingRunTimeData();

        for (int i = 0; i < blessings.Length; i++)
        {
            playerController.SkillOutlineUpdate?.Invoke((UIIcon)i, BlessingColor[(Blessings)blessings[i].skillData.Devil]);

            //switch ((Skills)i)
            //{
            //    case Skills.Mouse_L:
            //        animator.SetInteger("basicAttackBlessing", blessings[i].skillData.Devil);
            //        break;
            //    case Skills.Mouse_R:
            //        animator.SetInteger("mouseRightBlessing", blessings[i].skillData.Devil);
            //        break;
            //    case Skills.Space:
            //        animator.SetInteger("spaceBlessing", blessings[i].skillData.Devil);
            //        switch ((Blessings)blessings[i].skillData.Devil)
            //        {
            //            case Blessings.Gremory:
            //                break;
            //        }
            //        break;
            //    case Skills.Shift_L:
            //        animator.SetInteger("skillBlessing", blessings[i].skillData.Devil);
            //        break;
            //    case Skills.R:
            //        animator.SetInteger("ultimateBlessing", blessings[i].skillData.Devil);
            //        break;
            //}
        }
    }

    public void UpdateBlessing(SkillWithLevel data)
    {
        playerController.UpdateBlessingRunTimeData(data);

        if (data.level == 1)
        {
            playerController.SkillOutlineUpdate?.Invoke((UIIcon)data.skillData.Bind_Key, BlessingColor[(Blessings)data.skillData.Devil]);

            // 갱신된 가호에 맞게 스킬 변경 코드 추가 해야함
            //switch ((Skills)data.skillData.Bind_Key)
            //{
            //    case Skills.Mouse_L:
            //        animator.SetInteger("basicAttackBlessing", data.skillData.Devil);
            //        break;
            //    case Skills.Mouse_R:
            //        animator.SetInteger("mouseRightBlessing", data.skillData.Devil);
            //        break;
            //    case Skills.Space:
            //        animator.SetInteger("spaceBlessing", data.skillData.Devil);
            //        break;
            //    case Skills.Shift_L:
            //        animator.SetInteger("skillBlessing", data.skillData.Devil);
            //        switch ((Blessings)data.skillData.Devil)
            //        {
            //            case Blessings.Gremory:
            //                break;
            //        }
            //        break;
            //    case Skills.R:
            //        animator.SetInteger("ultimateBlessing", data.skillData.Devil);
            //        break;
            //}
        }
    }

    public SkillWithLevel[] ReturnSkillWithLevel()
    {
        return playerController.ReturnBlessingRunTimeData();
    }


    public BaseSpecialEffect FindSkillEffect(int skillID, ParentPlayerController playerController)
    {
        int specialEffectID = DataManager.Instance.FindSpecialEffectIDBySkillID(skillID);
        int linkID = DataManager.Instance.FindLinkIDBySkillID(skillID);

        if(specialEffectID < 0 || linkID < 0)
        {
            return null;
        }

        else
        {
            specialEffectList[specialEffectID].Init(DataManager.Instance.linkList[linkID].Value, DataManager.Instance.linkList[linkID].Duration, playerController);
            return specialEffectList[specialEffectID];
        }
    }
}
