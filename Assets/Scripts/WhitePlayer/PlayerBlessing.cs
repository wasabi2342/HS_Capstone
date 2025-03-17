using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R, Max }
public enum Blessings { Crocell, Gremory, Paymon, Max }
public class PlayerBlessing : MonoBehaviour
{
    private Dictionary<Skills, (Blessings, int)> playerBlessingDic = new Dictionary<Skills, (Blessings, int)>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //playerBlessingDic.Add(Skills.Mouse_L, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Mouse_R, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Space, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Shift_L, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.R, (Blessings.None, 0));
    }
    
    public void InitBlessing()
    {
        playerBlessingDic = new Dictionary<Skills, (Blessings, int)>();
    }

    public void UpdateBlessing(KeyValuePair<Skills, (Blessings, int)> data)
    {
        playerBlessingDic[data.Key] = data.Value;
        
        // 갱신된 가호에 맞게 스킬 변경 코드 추가 해야함
    }
    
    public Dictionary<Skills, (Blessings, int)> ReturnBlessingDic()
    {
        return playerBlessingDic;
    }
}
