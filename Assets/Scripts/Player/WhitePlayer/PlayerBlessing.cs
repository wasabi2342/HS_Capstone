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

    private Dictionary<Skills, BlessingInfo> playerBlessingDic = new Dictionary<Skills, BlessingInfo>();

    private void Awake()
    {
        playerController = GetComponent<ParentPlayerController>();
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
        // 갱신된 가호에 맞게 스킬 변경 코드 추가 해야함
        switch (data.Key)
        {
            case Skills.Mouse_L:
                switch (data.Value.blessing)
                {
                    case Blessings.Crocell:
                        break;
                    case Blessings.Gremory:
                        break;
                    case Blessings.Paymon:
                        break;
                }
                break;
            case Skills.Mouse_R:
                switch (data.Value.blessing)
                {
                    case Blessings.Crocell:
                        break;
                    case Blessings.Gremory:
                        break;
                    case Blessings.Paymon:
                        break;
                }
                break;
            case Skills.Space:
                switch (data.Value.blessing)
                {
                    case Blessings.Crocell:
                        break;
                    case Blessings.Gremory:
                        break;
                    case Blessings.Paymon:
                        break;
                }
                break;
            case Skills.Shift_L:
                switch (data.Value.blessing)
                {
                    case Blessings.Crocell:
                        break;
                    case Blessings.Gremory:
                        break;
                    case Blessings.Paymon:
                        break;
                }
                break;
            case Skills.R:
                switch (data.Value.blessing)
                {
                    case Blessings.Crocell:
                        break;
                    case Blessings.Gremory:
                        break;
                    case Blessings.Paymon:
                        break;
                }
                break;
        }
    }

    public Dictionary<Skills, BlessingInfo> ReturnBlessingDic()
    {
        return playerBlessingDic;
    }
}
