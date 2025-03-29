using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R, Max }
public enum Blessings { None, Crocell, Gremory, Paymon, Max }

[Serializable]
public class BlessingInfo
{
    public Blessings blessing;
    public int level;

    public BlessingInfo(Blessings blessing, int level)
    {
        this.blessing = blessing;
        this.level = level;
    }
}
public class PlayerBlessing : MonoBehaviourPun
{
    [SerializeField]
    private ParentPlayerController playerController;
    private Animator animator;
    private Dictionary<Skills, BlessingInfo> playerBlessingDic = new Dictionary<Skills, BlessingInfo>();

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

        BlessingInfo[] blessings = playerController.ReturnBlessingRunTimeData();
        for (int i = 0; i < blessings.Length; i++)
        {
            playerBlessingDic[(Skills)i] = blessings[i];
            playerController.SkillOutlineUpdate.Invoke((UIIcon)i, BlessingColor[blessings[i].blessing]);
            switch ((Skills)i)
            {
                case Skills.Mouse_L:
                    animator.SetInteger("basicAttackBlessing", (int)blessings[i].blessing);
                    break;
                case Skills.Mouse_R:
                    animator.SetInteger("mouseRightBlessing", (int)blessings[i].blessing);
                    break;
                case Skills.Space:
                    animator.SetInteger("spaceBlessing", (int)blessings[i].blessing);
                    break;
                case Skills.Shift_L:
                    animator.SetInteger("skillBlessing", (int)blessings[i].blessing);
                    break;
                case Skills.R:
                    animator.SetInteger("ultimateBlessing", (int)blessings[i].blessing);
                    break;
            }
        }
    }

    public void InitBlessing()
    {
        playerBlessingDic = new Dictionary<Skills, BlessingInfo>();
    }

    public void UpdateBlessing(KeyValuePair<Skills, BlessingInfo> data)
    {
        playerBlessingDic[data.Key] = data.Value;
        playerController.UpdateBlessingRunTimeData(playerBlessingDic);
        playerController.SkillOutlineUpdate.Invoke((UIIcon)data.Key, BlessingColor[data.Value.blessing]);
        // 갱신된 가호에 맞게 스킬 변경 코드 추가 해야함
        switch (data.Key)
        {
            case Skills.Mouse_L:
                animator.SetInteger("basicAttackBlessing", (int)data.Value.blessing);
                break;
            case Skills.Mouse_R:
                animator.SetInteger("mouseRightBlessing", (int)data.Value.blessing);
                break;
            case Skills.Space:
                animator.SetInteger("spaceBlessing", (int)data.Value.blessing);
                break;
            case Skills.Shift_L:
                animator.SetInteger("skillBlessing", (int)data.Value.blessing);
                break;
            case Skills.R:
                animator.SetInteger("ultimateBlessing", (int)data.Value.blessing);
                break;
        }
    }

    public Dictionary<Skills, BlessingInfo> ReturnBlessingDic()
    {
        return playerBlessingDic;
    }
}
