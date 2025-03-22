using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R, Max }
public enum Blessings { Crocell, Gremory, Paymon, Max }

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
        for(int i = 0; i < blessings.Length; i++)
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
        // ���ŵ� ��ȣ�� �°� ��ų ���� �ڵ� �߰� �ؾ���
    }
    
    public Dictionary<Skills, BlessingInfo> ReturnBlessingDic()
    {
        return playerBlessingDic;
    }
}
