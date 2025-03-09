using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Skills { Mouse_L, Mouse_R, Space, Shift_L, R, Max }
public enum Blessings { Crocell, Gremory, Paymon, Max }
public class PlayerBlessing : MonoBehaviour
{
    public Dictionary<Skills, (Blessings, int)> playerBlessingDic = new Dictionary<Skills, (Blessings, int)>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //playerBlessingDic.Add(Skills.Mouse_L, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Mouse_R, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Space, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.Shift_L, (Blessings.None, 0));
        //playerBlessingDic.Add(Skills.R, (Blessings.None, 0));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
