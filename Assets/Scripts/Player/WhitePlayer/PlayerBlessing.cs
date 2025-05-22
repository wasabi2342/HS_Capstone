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
        }
    }

    public void UpdateBlessing(SkillWithLevel data)
    {
        playerController.UpdateBlessingRunTimeData(data);

        if (data.level == 1)
        {
            playerController.SkillOutlineUpdate?.Invoke((UIIcon)data.skillData.Bind_Key, BlessingColor[(Blessings)data.skillData.Devil]);
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

    public void TryApplyDebuffOnHit(SkillWithLevel skillWithLevel, EnemyFSM fsm)
    {
        if (skillWithLevel == null || fsm == null)
            return;

        if (skillWithLevel.level <= 0)
            return;

        var effectType = skillWithLevel.skillData.debuffType;
        if (effectType == SpecialEffectType.None)
            return;

        float duration = skillWithLevel.skillData.debuffDuration;
        float value = skillWithLevel.skillData.debuffValue;

        if (fsm.debuff != null)
        {
            fsm.debuff.ApplyDebuff(effectType, duration, value);
        }
    }

}
